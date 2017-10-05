using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.PO;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CM;
using PX.Objects.CS;
using System.Collections;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.AR;
using PX.Objects.GL;
using PX.TM;
using PX.Objects;
using PX.Objects.RQ;

namespace PX.Objects.RQ
{
  
  public class RQRequisitionEntry_Extension:PXGraphExtension<RQRequisitionEntry>
  {

    #region Event Handlers

            
// ------------------------------------------------------------------------------------------------------------------------------------------------------------
    public PXAction<PX.Objects.RQ.RQRequisition> createQuote;
    [PXButton(CommitChanges = true)]
    [PXUIField(DisplayName = "Create Quote")]
    public virtual IEnumerable CreateQuote(PXAdapter adapter)
    {
      SOOrderEntry sograph = PXGraph.CreateInstance<SOOrderEntry>();
      List<SOOrder> list = new List<SOOrder>();
      foreach (RQRequisition item in adapter.Get<RQRequisition>())
      {
        RQRequisition result = item;
        RQRequisitionOrder req =
        PXSelectJoin<RQRequisitionOrder,
          InnerJoin<SOOrder,
                 On<SOOrder.orderNbr, Equal<RQRequisitionOrder.orderNbr>,
                And<SOOrder.status, Equal<SOOrderStatus.open>>>>,
          Where<RQRequisitionOrder.reqNbr, Equal<Required<RQRequisitionOrder.reqNbr>>,
            And<RQRequisitionOrder.orderCategory, Equal<RQOrderCategory.so>>>>
            .Select(Base, item.ReqNbr);
  
        if (item.CustomerID != null && req == null)
        {
          Base.Document.Current = item;
  
          bool validateResult = true;
          foreach (RQRequisitionLine line in Base.Lines.Select(item.ReqNbr))
          {
            if (!ValidateOpenState(line, PXErrorLevel.Error))
              validateResult = false;
          }
          if (!validateResult)
            throw new PXRowPersistingException(typeof(RQRequisition).Name, item, Messages.UnableToCreateOrders);
  
          sograph.TimeStamp = Base.TimeStamp;
          sograph.Document.Current = null;
          foreach (PXResult<RQRequisitionLine, InventoryItem> r in
            PXSelectJoin<RQRequisitionLine,
            LeftJoin<InventoryItem,
                  On<InventoryItem.inventoryID, Equal<RQRequisitionLine.inventoryID>>>,
            Where<RQRequisitionLine.reqNbr, Equal<Required<RQRequisition.reqNbr>>>>.Select(Base, item.ReqNbr))
          {
            RQRequisitionLine l = r;
            InventoryItem i = r;
            RQBidding bidding =
              item.VendorID == null ?
              PXSelect<RQBidding,
              Where<RQBidding.reqNbr, Equal<Current<RQRequisitionLine.reqNbr>>,
                And<RQBidding.lineNbr, Equal<Current<RQRequisitionLine.lineNbr>>,
                And<RQBidding.orderQty, Greater<decimal0>>>>,
                OrderBy<Desc<RQBidding.quoteUnitCost>>>
                .SelectSingleBound(Base, new object[] { l }) :
              PXSelect<RQBidding,
              Where<RQBidding.reqNbr, Equal<Current<RQRequisitionLine.reqNbr>>,
                And<RQBidding.lineNbr, Equal<Current<RQRequisitionLine.lineNbr>>,
                And<RQBidding.vendorID, Equal<Current<RQRequisition.vendorID>>,
                And<RQBidding.vendorLocationID, Equal<Current<RQRequisition.vendorLocationID>>>>>>>
                .SelectSingleBound(Base, new object[] { l, item });
  
            if (sograph.Document.Current == null)
            {
              SOOrder order = (SOOrder)sograph.Document.Cache.CreateInstance();
              order.OrderType = "QT";
              order = sograph.Document.Insert(order);
              order = PXCache<SOOrder>.CreateCopy(sograph.Document.Search<SOOrder.orderNbr>(order.OrderNbr));
              order.CustomerID = item.CustomerID;
              order.CustomerLocationID = item.CustomerLocationID;
              order = PXCache<SOOrder>.CreateCopy(sograph.Document.Update(order));
              order.CuryID = item.CuryID;
              order.CuryInfoID = CopyCurrenfyInfo(sograph, item.CuryInfoID);
              sograph.Document.Update(order);
              sograph.Save.Press();
              order = sograph.Document.Current;
              list.Add(order);
  
              RQRequisitionOrder link = new RQRequisitionOrder();
              link.OrderCategory = RQOrderCategory.SO;
              link.OrderType = order.OrderType;
              link.OrderNbr = order.OrderNbr;
              Base.ReqOrders.Insert(link);              
            }
            SOLine line = (SOLine)sograph.Transactions.Cache.CreateInstance();
            line.OrderType = sograph.Document.Current.OrderType;
            line.OrderNbr = sograph.Document.Current.OrderNbr;
            line = PXCache<SOLine>.CreateCopy(sograph.Transactions.Insert(line));
            line.InventoryID = l.InventoryID;
            if(line.InventoryID != null)
              line.SubItemID = l.SubItemID;
            line.UOM = l.UOM;
            line.Qty = l.OrderQty;
            if (l.SiteID != null)
              line.SiteID = l.SiteID;
  
            if (l.IsUseMarkup == true)
            {
              string curyID = item.CuryID;
              decimal profit = (1m + l.MarkupPct.GetValueOrDefault() / 100m);
              line.ManualPrice = true;
              decimal unitPrice = l.EstUnitCost.GetValueOrDefault();
              decimal curyUnitPrice = l.CuryEstUnitCost.GetValueOrDefault();
              decimal curyTotalCost = l.CuryEstExtCost.GetValueOrDefault();
              decimal totalCost = l.EstExtCost.GetValueOrDefault();
  
              if (bidding != null && bidding.MinQty <= line.OrderQty && bidding.OrderQty >= line.OrderQty)
              {
                curyID = (string)Base.Bidding.GetValueExt<RQBidding.curyID>(bidding);
                unitPrice = bidding.QuoteUnitCost.GetValueOrDefault();
                curyUnitPrice = bidding.CuryQuoteUnitCost.GetValueOrDefault();
                curyTotalCost = l.CuryEstExtCost.GetValueOrDefault();
                totalCost = l.EstExtCost.GetValueOrDefault();
              }
  
              if (curyID == sograph.Document.Current.CuryID)
                //line.CuryUnitPrice = curyUnitPrice * profit;
                line.CuryUnitPrice = (curyTotalCost / l.OrderQty) * profit;
              else
              {
                //line.UnitPrice = unitPrice * profit;
                line.UnitPrice = (totalCost / l.OrderQty) * profit;
                PXCurrencyAttribute.CuryConvCury<SOLine.curyUnitPrice>(
                  sograph.Transactions.Cache,
                  line);
              }
            }
  
            line = PXCache<SOLine>.CreateCopy(sograph.Transactions.Update(line));
            RQRequisitionLine upd = PXCache<RQRequisitionLine>.CreateCopy(l);
            l.QTOrderNbr = line.OrderNbr;
            l.QTLineNbr = line.LineNbr;
            Base.Lines.Update(l);
          }
          using (PXTransactionScope scope = new PXTransactionScope())
          {
            try
            {
              if (sograph.IsDirty) sograph.Save.Press();
              RQRequisition upd = PXCache<RQRequisition>.CreateCopy(item);
              upd.Quoted = true;
              result = Base.Document.Update(upd);
              Base.Save.Press();
            }
            catch
            {
              Base.Clear();
              throw;
            }
            scope.Complete();
          }
        }
        else
        {
          RQRequisition upd = PXCache<RQRequisition>.CreateCopy(item);
          upd.Quoted = true;
          result = Base.Document.Update(upd);
          Base.Save.Press();
        }
        yield return result;
      }
      if(list.Count == 1 && adapter.MassProcess == true)
      {
        sograph.Clear();
        sograph.SelectTimeStamp();
        sograph.Document.Current = list[0];
        throw new PXRedirectRequiredException(sograph, SO.Messages.SOOrder);
      }
    }

    private bool ValidateOpenState(RQRequisitionLine row, PXErrorLevel level)
    {
      bool result = true;
      Type[] requestOnOpen =
        row.LineType == POLineType.GoodsForInventory && row.InventoryID != null
          ? new Type[] {typeof (RQRequisitionLine.uOM), typeof (RQRequisitionLine.siteID), typeof (RQRequisitionLine.subItemID)}
          : row.LineType == POLineType.NonStock
              ? new Type[] {typeof (RQRequisitionLine.uOM), typeof (RQRequisitionLine.siteID),}
              : new Type[] {typeof (RQRequisitionLine.uOM)};
  
  
      foreach (Type type in requestOnOpen)
      {
        object value = Base.Lines.Cache.GetValue(row, type.Name);
        if (value == null)
        {
          Base.Lines.Cache.RaiseExceptionHandling(type.Name, row, null,
            new PXSetPropertyException(Messages.ShouldBeDefined, level));
          result = false;
        }
        else
          Base.Lines.Cache.RaiseExceptionHandling(type.Name, row, value, null);
      }
      return result;
    }
    
    
    private long? CopyCurrenfyInfo(PXGraph graph, long? SourceCuryInfoID)
    {
      CurrencyInfo curryInfo = Base.currencyinfo.Select(SourceCuryInfoID);
      curryInfo.CuryInfoID = null;
      graph.Caches[typeof (CurrencyInfo)].Clear();
      curryInfo = (CurrencyInfo)graph.Caches[typeof(CurrencyInfo)].Insert(curryInfo);
      return curryInfo.CuryInfoID;
    }
      
// ------------------------------------------------------------------------------------------------------------------------------------------------------------
    public Boolean RequesRefresh = false; 

    public PXAction<RQRequisition> createQTOrder;
    [PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
    [PXUIField(DisplayName = Messages.CreateQuotation)]
    public virtual IEnumerable CreateQTOrder(PXAdapter adapter)
    {
      PXGraph.InstanceCreated.AddHandler<SOOrderEntry>(delegate(SOOrderEntry graph)
      {
        Base.FieldUpdated.AddHandler<RQRequisitionLine.qTOrderNbr>(delegate(PXCache cache, PXFieldUpdatedEventArgs e)
        {
          RQRequisition req = Base.Document.Current;
          RQRequisitionLine reqline = (RQRequisitionLine)e.Row;
          if (reqline.QTOrderNbr != null && reqline.QTLineNbr != null)
          {
            SOOrder order = graph.Document.Current;
            SOLine line = PXSelect<SOLine,
              Where<SOLine.orderType, Equal<SOOrderTypeConstants.quoteOrder>,
                And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
                And<SOLine.lineNbr, Equal<Required<SOLine.lineNbr>>>>>>
                  .Select(graph, reqline.QTOrderNbr, reqline.QTLineNbr);
            if(line != null)
            {
              decimal profit = (1m + reqline.MarkupPct.GetValueOrDefault() / 100m);
              decimal unitcost = (reqline.EstExtCost ?? 0m) / (reqline.OrderQty ?? 0m);
              decimal extcost = reqline.EstExtCost ?? 0m;

              if (reqline.IsUseMarkup == true)
              {
                unitcost = unitcost * profit;
                extcost = extcost * profit;
              }
              if (req.CuryID != order.CuryID)
              {
                unitcost = Tools.ConvertCurrency<SOLine.curyInfoID>(cache, line, unitcost);
                extcost = Tools.ConvertCurrency<SOLine.curyInfoID>(cache, line, extcost);
              }

              line.ManualPrice = true;
              graph.Transactions.Cache.SetValueExt<SOLine.curyUnitPrice>(line, unitcost);
              line = graph.Transactions.Update(line);
              graph.Transactions.Cache.SetValueExt<SOLine.curyExtPrice>(line, extcost);
              line = graph.Transactions.Update(line);
            }
          }
        });
      });
      return Base.CreateQTOrder(adapter);
    }

    public Boolean preventRecursion = false;
    public PXAction<RQRequisition> createPOOrder;
    [PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
    [PXUIField(DisplayName = Messages.CreateOrders)]
    public virtual IEnumerable CreatePOOrder(PXAdapter adapter)
    {
      PXGraph.InstanceCreated.AddHandler<POOrderEntry>(delegate(POOrderEntry graph)
      {
        //graph.RowUpdated.AddHandler<POLine>(delegate(PXCache cache, PXRowUpdatedEventArgs e)
        //{
        //  POLine poline = (POLine)e.Row;
        //  if (!preventRecursion && poline.RQReqNbr != null && poline.RQReqLineNbr != null)
        //  {
        //    RQRequisitionLine line = PXSelect<RQRequisitionLine,
        //      Where<RQRequisitionLine.reqNbr, Equal<Required<RQRequisitionLine.reqNbr>>,
        //        And<RQRequisitionLine.lineNbr, Equal<Required<RQRequisitionLine.lineNbr>>>>>
        //          .Select(graph, poline.RQReqNbr, poline.RQReqLineNbr);
        //    if (line != null)
        //    {
        //      RQRequisitionLineExt lineext = line.GetExtension<RQRequisitionLineExt>();

        //      try
        //      {
        //        preventRecursion = true;

        //        Decimal amt = line.OrderQty > 0 ? (lineext.UsrIntermediateCost ?? 0) / (line.OrderQty ?? 1) : (lineext.UsrIntermediateCost ?? 0);
        //        amt = Tools.ConvertCurrency<POLine.curyInfoID>(cache, line, amt);
        //        graph.Transactions.Cache.SetValueExt<POLine.curyUnitCost>(poline, amt);

        //        amt = Tools.ConvertCurrency<POLine.curyInfoID>(cache, line, lineext.UsrIntermediateCost ?? 0);
        //        graph.Transactions.Cache.SetValueExt<POLine.curyExtCost>(poline, amt);

        //        poline = graph.Transactions.Update(poline);
        //      }
        //      finally
        //      {
        //        preventRecursion = false;
        //      }
        //    }
        //  }
        //});
      });
      Base.RowSelecting.AddHandler<RQBidding>(delegate(PXCache cache, PXRowSelectingEventArgs e)
      {
        RQBidding row = e.Row as RQBidding;
        RQBiddingExt rowext = row.GetExtension<RQBiddingExt>();

        if (row.CuryID == null)
        {
          using (new PXConnectionScope())
          {
            CurrencyInfo ci = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(Base, row.CuryInfoID);
            if (ci != null)
            {
              row.CuryID = ci.CuryID;
            }
          }
          row.CuryQuoteUnitCost = row.QuoteQty > 0 ? rowext.CuryQuoteExtCost / row.QuoteQty : rowext.CuryQuoteExtCost;
        }
      });
      return Base.CreatePOOrder(adapter);
    }

    protected virtual void RQRequisition_VendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
    {
      RQRequisition row = e.Row as RQRequisition; 

      foreach(RQRequisitionLine line in Base.Lines.Select())
      {
        RQRequisitionLineExt lineext = Base.Lines.Cache.GetExtension<RQRequisitionLineExt>(line);
        if (row.VendorID == null || row.VendorLocationID == null)
        {
          lineext.UsrPatternCost = 0;
          lineext.UsrCuryPatternCost = 0;
        }
        else
        {
          RQBidding bidding = PXSelect<RQBidding,
            Where<RQBidding.reqNbr, Equal<Required<RQBidding.reqNbr>>,
              And<RQBidding.lineNbr, Equal<Required<RQBidding.lineNbr>>,
              And<RQBidding.vendorID, Equal<Required<RQBidding.vendorID>>,
              And<RQBidding.vendorLocationID, Equal<Required<RQBidding.vendorLocationID>>>>>>>.Select(Base, line.ReqNbr, line.LineNbr, row.VendorID, row.VendorLocationID);
          if (bidding != null)
          {
            RQBiddingExt biddingext = bidding.GetExtension<RQBiddingExt>();

            //Pattern
            lineext.UsrCuryPatternCost = Tools.ConvertCurrency<RQRequisitionLine.curyInfoID>(Base.Lines.Cache, line, biddingext.UsrPatternCost ?? 0);
            //Unit Cost
            line.CuryEstUnitCost = Tools.ConvertCurrency<RQRequisitionLine.curyInfoID>(Base.Lines.Cache, line, bidding.QuoteUnitCost ?? 0);
            //Ext Cost
            line.CuryEstExtCost = Tools.ConvertCurrency<RQRequisitionLine.curyInfoID>(Base.Lines.Cache, line, bidding.QuoteExtCost ?? 0);
          }
        }
        Base.Lines.Update(line);
      }
    }

    protected virtual void RQRequisition_UsrCuryEngineeringCost_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
    {
      CalculateAdditioanCost();
    }
    protected virtual void RQRequisition_UsrCuryShippingCost_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
    {
      CalculateAdditioanCost();
    }
    protected virtual void RQRequisition_UsrCuryCustomsClearanceCost_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
    {
      CalculateAdditioanCost();
    }
    protected virtual void RQRequisition_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
    {
      if (RequesRefresh)
      {
        CalculateAdditioanCost();
        Base.Lines.View.RequestRefresh();
        RequesRefresh = false;   
      }
    }

    protected virtual void RQRequisitionLine_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
    {
      if (!sender.ObjectsEqual<RQRequisitionLineExt.usrCuryIntermediateCost, RQRequisitionLineExt.usrCuryPatternCost>(e.Row, e.OldRow))
      {
        RequesRefresh = true;
      }
    }
    protected virtual void RQRequisitionLine_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
    {
      CalculateAdditioanCost();
    }

    public virtual void CalculateAdditioanCost()
    {
      RQRequisition req = Base.Document.Current;
      RQRequisitionExt reqext = req.GetExtension<RQRequisitionExt>();

      Decimal intermediate = 0;
      List<RQRequisitionLine> lines = new List<RQRequisitionLine>();
      foreach (RQRequisitionLine line in Base.Lines.Select())
      {
        RQRequisitionLineExt lineext = line.GetExtension<RQRequisitionLineExt>();
        intermediate += lineext.UsrCuryIntermediateCost ?? 0;

        lines.Add(line);
      }

      Decimal additional = (reqext.UsrCuryCustomsClearanceCost ?? 0) + (reqext.UsrCuryEngineeringCost ?? 0) + (reqext.UsrCuryShippingCost ?? 0);
      Decimal running = 0;
      for (int i = 0; i < lines.Count; i++)
      {
        RQRequisitionLine line = lines[i];
        RQRequisitionLineExt lineext = line.GetExtension<RQRequisitionLineExt>();

        if (intermediate <= 0)
        {
          lineext.UsrCuryAdditionalCost = 0;
        }
        else
        {
          if (i >= (lines.Count - 1))
          {
            lineext.UsrCuryAdditionalCost = additional - running;
          }
          else
          {
            Decimal val = additional * ((lineext.UsrCuryIntermediateCost ?? 0) / intermediate);
            lineext.UsrCuryAdditionalCost = PXDBCurrencyAttribute.RoundCury<RQRequisitionLine.curyInfoID>(Base.Lines.Cache, line, val);
            running += lineext.UsrCuryAdditionalCost ?? 0;
          }
        }

        Base.Lines.Cache.Update(line);
      }
    }


    #endregion

  }


}