using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.CM;

namespace PX.Objects.RQ
{
  public class Tools
  {
    public static decimal ConvertCurrency<Field>(PXCache cache, Object row, Decimal val)
      where Field :IBqlField
    {
      Decimal curyval = 0;
      PXCurrencyAttribute.CuryConvCury<Field>(cache, row, val, out curyval);
      return curyval;
    }
  }
}