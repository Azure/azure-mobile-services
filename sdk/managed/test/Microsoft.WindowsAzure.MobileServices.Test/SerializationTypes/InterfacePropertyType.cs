// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class InterfacePropertyType
    {
        public int Id { get; set; }

        public IDictionary<string, string> Lookup { get; set; }

        public override bool Equals(object obj)
        {
            InterfacePropertyType other = obj as InterfacePropertyType;
            if (other != null)
            {
                if (other.Id == this.Id)
                {
                    if ((this.Lookup == null && other.Lookup == null) ||
                        (this.Lookup.Count == other.Lookup.Count &&
                        this.Lookup.SequenceEqual(other.Lookup))
                        )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
