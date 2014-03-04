// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service;
using Newtonsoft.Json;
using System;

namespace ZumoE2EServerApp.DataObjects
{
    public class StringIdMovie : EntityData
    {
        public string Title { get; set; }
        public int Duration { get; set; }
        public string MPAARating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool BestPictureWinner { get; set; }
        public int Year { get; set; }
    }

    public class AllStringIdMovies
    {
        public string Status { get; set; }
        public StringIdMovie[] Movies { get; set; }
    }
}
