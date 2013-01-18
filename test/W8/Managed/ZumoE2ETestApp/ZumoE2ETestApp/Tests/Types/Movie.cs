using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using ZumoE2ETestApp.Framework;

namespace ZumoE2ETestApp.Tests.Types
{
    [DataTable(Name = ZumoTestGlobals.MoviesTableName)]
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public string MPAARating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool BestPictureWinner { get; set; }
        public int Year { get; set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Movie[Title={0},Duration={1},Rating={2},ReleaseDate={3},BestPictureWinner={4},Year={5}",
                Title, Duration, MPAARating,
                ReleaseDate.ToUniversalTime().ToString("yyyy-MM-dd:HH:mm:ss.fffZ", CultureInfo.InvariantCulture),
                BestPictureWinner, Year);
        }

        public override int GetHashCode()
        {
            int result = 0;
            if (Title != null) result ^= Title.GetHashCode();
            result ^= Duration.GetHashCode();
            if (MPAARating != null) result ^= MPAARating.GetHashCode();
            result ^= ReleaseDate.ToUniversalTime().GetHashCode();
            result ^= BestPictureWinner.GetHashCode();
            result ^= Year.GetHashCode();
            return result;
        }

        public override bool Equals(object obj)
        {
            Movie other = obj as Movie;
            if (other == null) return false;
            if (this.Title != other.Title) return false;
            if (this.Duration != other.Duration) return false;
            if (this.MPAARating != other.MPAARating) return false;
            if (!this.ReleaseDate.ToUniversalTime().Equals(other.ReleaseDate.ToUniversalTime())) return false;
            if (this.BestPictureWinner != other.BestPictureWinner) return false;
            if (this.Year != other.Year) return false;
            return true;
        }
    }

    [DataTable(Name = ZumoTestGlobals.MoviesTableName)]
    public class AllMovies
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }
        [DataMember(Name = "status")]
        public string Status { get; set; }
        [DataMember(Name = "movies")]
        [DataMemberJsonConverter(ConverterType = typeof(MovieArrayConverter))]
        public Movie[] Movies { get; set; }
    }

    public class MovieArrayConverter : IDataMemberJsonConverter
    {
        public object ConvertFromJson(IJsonValue value)
        {
            // unused
            return null;
        }

        public IJsonValue ConvertToJson(object instance)
        {
            Movie[] movies = (Movie[])instance;
            JsonArray result = new JsonArray();
            foreach (var movie in movies)
            {
                result.Add(MobileServiceTableSerializer.Serialize(movie));
            }

            return result;
        }
    }
}
