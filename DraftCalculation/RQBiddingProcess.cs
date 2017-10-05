using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.PO;
using PX.Objects.AP;
using System.Collections;
using PX.Objects.CR;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects;
using PX.Objects.RQ;
using PX.Objects.SO;

namespace PX.Objects.RQ
{
  public class RQBiddingProcess_Extension : PXGraphExtension<RQBiddingProcess>
  {
    protected virtual void RQRequisition_VendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
    {
      RQRequisition upd = e.Row as RQRequisition;

      EnsurePattern(e.Row as RQRequisition);
    }
    protected virtual void RQRequisition_CuryRateTypeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
    {
      EnsurePattern(e.Row as RQRequisition);
    }
    protected virtual void RQRequisition_CuryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
    {
      EnsurePattern(e.Row as RQRequisition);
    }

    protected virtual void RQRequisitionLine_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
    {
      RQRequisitionLine row = e.Row as RQRequisitionLine;
      RQRequisitionLineExt rowext = row.GetExtension<RQRequisitionLineExt>();

      if (rowext.PreventUpdate == true)
      {
        e.Cancel = true;
        rowext.PreventUpdate = false;
      }
    }
    protected virtual void RQRequisitionLine_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
    {
      //RQRequisition parent = Base.Document.Current;
      //RQRequisitionLine row = e.Row as RQRequisitionLine;
      //if (!preventRecursion
      //  //&& !sender.ObjectsEqual<RQRequisitionLine.curyEstUnitCost, RQRequisitionLineExt.curyEstExtCost, RQRequisitionLineExt.usrCuryPatternCost>(e.Row, e.OldRow)
      //  )
      //{
      //  try
      //  {
      //    preventRecursion = true;
      //    EnsureLine(parent, row);
      //    PXFormulaAttribute.CalcAggregate<RQRequisitionLineExt.curyEstExtCost>(sender, parent);
      //  }
      //  finally
      //  {
      //    preventRecursion = false;
      //  }
      //}
    }

    protected virtual void EnsurePattern(RQRequisition row)
    {
      foreach (RQRequisitionLine line in Base.Lines.Select())
      {
        RQRequisitionLineExt lineext = Base.Lines.Cache.GetExtension<RQRequisitionLineExt>(line);

        EnsureLine(row, line, lineext);
        Base.Lines.Cache.Update(line);

        lineext.PreventUpdate = true;
      }
    }
    protected virtual void EnsureLine(RQRequisition row, RQRequisitionLine line, RQRequisitionLineExt lineext)
    {
      if (row.VendorID == null || row.VendorLocationID == null)
      {
        lineext.UsrPatternCost = 0;
        lineext.UsrCuryPatternCost = 0;
      }
      else
      {
        PXCache cache = Base.Bidding.Cache;
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
    }
  }
}