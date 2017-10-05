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

namespace PX.Objects.RQ
{
  public class RQBiddingEntry_Extension : PXGraphExtension<RQBiddingEntry>
  {
    protected void RQRequisitionLineBidding_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
    {
      var row = (RQRequisitionLineBidding)e.Row;
      PXUIFieldAttribute.SetEnabled<RQRequisitionLineBiddingExt.usrCuryPatternCost>(cache, row, true);
    }

    public PXSelect<RQRequisitionLineBidding,
      Where<RQRequisitionLineBidding.reqNbr, Equal<Current<RQBiddingVendor.reqNbr>>>> Lines;
    protected virtual IEnumerable lines()
    {
      if (Base.Vendor.Current == null || Base.Vendor.Current.VendorLocationID == null)
        yield break;
      using (ReadOnlyScope scope = new ReadOnlyScope(this.Lines.Cache))
      {
        bool reset = !Base.Bidding.Cache.IsDirty;
        PXResultset<RQRequisitionLineBidding> list =
          PXSelectJoin<RQRequisitionLineBidding,
            LeftJoin<RQBidding,
                  On<RQBidding.reqNbr, Equal<RQRequisitionLineBidding.reqNbr>,
                 And<RQBidding.lineNbr, Equal<RQRequisitionLineBidding.lineNbr>,
                And<RQBidding.vendorID, Equal<Current<RQBiddingVendor.vendorID>>,
                And<RQBidding.vendorLocationID, Equal<Current<RQBiddingVendor.vendorLocationID>>>>>>>,
            Where<RQRequisitionLineBidding.reqNbr, Equal<Current<RQBiddingVendor.reqNbr>>>>
            .Select(Base);

        if (reset) this.Lines.Cache.Clear();
        foreach (PXResult<RQRequisitionLineBidding, RQBidding> item in list)
        {
          RQRequisitionLineBidding result = item;
          RQBidding bidding = item;
          bidding = Base.Bidding.Locate(bidding) ?? item;
          RQBiddingExt biddingExt = Base.Bidding.Cache.GetExtension<RQBiddingExt>(bidding);

          result = (RQRequisitionLineBidding)this.Lines.Cache.CreateCopy(result);
          RQRequisitionLineBiddingExt resultExt = (RQRequisitionLineBiddingExt)this.Lines.Cache.GetExtension<RQRequisitionLineBiddingExt>(result);

          result.QuoteNumber = bidding.QuoteNumber;
          result.QuoteQty = bidding.QuoteQty ?? 0m;
          result.CuryInfoID = Base.Vendor.Current.CuryInfoID;
          result.CuryQuoteUnitCost = bidding.CuryQuoteUnitCost ?? 0m;
          result.QuoteUnitCost = bidding.QuoteUnitCost ?? 0m;
          result.CuryQuoteExtCost = bidding.CuryQuoteExtCost ?? 0m;
          result.QuoteExtCost = bidding.QuoteExtCost ?? 0m;
          result.MinQty = bidding.MinQty ?? 0m;
          resultExt.UsrPatternCost = biddingExt.UsrPatternCost;
          resultExt.UsrCuryPatternCost = biddingExt.UsrCuryPatternCost;

          if (bidding.CuryQuoteUnitCost == null && result.InventoryID != null)
          {
            POItemCostManager.ItemCost cost =
              POItemCostManager.Fetch(Base,
                Base.Vendor.Current.VendorID,
                Base.Vendor.Current.VendorLocationID, null,
                (string)Base.Vendor.GetValueExt<RQBiddingVendor.curyID>(Base.Vendor.Current),
                result.InventoryID, result.SubItemID, null, result.UOM);
            result.CuryQuoteUnitCost =
              cost.Convert<RQRequisitionLineBidding.inventoryID, RQRequisitionLineBidding.curyInfoID>(Base, result, result.UOM); ;
          }

          if (result.CuryQuoteUnitCost == null)
            result.CuryQuoteUnitCost = 0m;

          result = this.Lines.Insert(result) ?? (RQRequisitionLineBidding)this.Lines.Cache.Locate(result);

          yield return result;
        }
      }
    }

    protected virtual void RQRequisitionLineBidding_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
    {
      RQRequisitionLineBidding newRow = (RQRequisitionLineBidding)e.Row;
      RQRequisitionLineBiddingExt newExt = (RQRequisitionLineBiddingExt)sender.GetExtension<RQRequisitionLineBiddingExt>(e.Row);
      RQRequisitionLineBidding oldRow = (RQRequisitionLineBidding)e.OldRow;
      RQRequisitionLineBiddingExt oldExt = (RQRequisitionLineBiddingExt)sender.GetExtension<RQRequisitionLineBiddingExt>(e.OldRow);

      //if (newExt.UsrPatternCost != oldExt.UsrPatternCost || newExt.UsrCuryPatternCost != oldExt.UsrCuryPatternCost)
      {
        RQBidding bidding =
          PXSelect<RQBidding,
          Where<RQBidding.reqNbr, Equal<Required<RQBidding.reqNbr>>,
          And<RQBidding.lineNbr, Equal<Required<RQBidding.lineNbr>>,
          And<RQBidding.vendorID, Equal<Required<RQBidding.vendorID>>,
          And<RQBidding.vendorLocationID, Equal<Required<RQBidding.vendorLocationID>>>>>>>.SelectWindowed(
          Base, 0, 1,
          Base.Vendor.Current.ReqNbr,
          newRow.LineNbr,
          Base.Vendor.Current.VendorID,
          Base.Vendor.Current.VendorLocationID);

        if (bidding == null)
        {
          bidding = new RQBidding();
          bidding.VendorID = Base.Vendor.Current.VendorID;
          bidding.VendorLocationID = Base.Vendor.Current.VendorLocationID;
          bidding.ReqNbr = Base.Vendor.Current.ReqNbr;
          bidding.CuryInfoID = Base.Vendor.Current.CuryInfoID;
          bidding.LineNbr = newRow.LineNbr;
        }
        else
          bidding = (RQBidding)Base.Bidding.Cache.CreateCopy(bidding);

        RQBiddingExt biddingExt = Base.Bidding.Cache.GetExtension<RQBiddingExt>(bidding);

        biddingExt.UsrPatternCost = newExt.UsrPatternCost;
        biddingExt.UsrCuryPatternCost = newExt.UsrCuryPatternCost;

        Base.Bidding.Update(bidding);
      }
    }
  }
}