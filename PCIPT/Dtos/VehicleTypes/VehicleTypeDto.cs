﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Dtos.VehicleTypes
{
    public sealed record VehicleTypeDto(string TypeName, List<string> AvaliableCargoTypes) : IData;
}
