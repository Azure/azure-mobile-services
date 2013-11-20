// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Linq;
using ZumoE2ETestApp.Tests.Types;

namespace ZumoE2ETestApp.Tests
{
    public static class ZumoQueryTestData
    {
        public static StringIdMovie[] AllStringIdMovies()
        {
            return AllMovies.Select((m, i) => new StringIdMovie(string.Format("Movie {0:000}", i), m)).ToArray();
        }

        public static Movie[] AllMovies = new Movie[]
        {
            new Movie
            {
                BestPictureWinner = false,
                Duration = 142,
                MPAARating = "R",
                ReleaseDate = new DateTime(1994, 10, 14, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Shawshank Redemption",
                Year = 1994
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 175,
                MPAARating = "R",
                ReleaseDate = new DateTime(1972, 03, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Godfather",
                Year = 1972
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 200,
                MPAARating = "R",
                ReleaseDate = new DateTime(1974, 12, 20, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Godfather: Part II",
                Year = 1974
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 168,
                MPAARating = "R",
                ReleaseDate = new DateTime(1994, 10, 14, 0, 0, 0, DateTimeKind.Utc),
                Title = "Pulp Fiction",
                Year = 1994
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 161,
                MPAARating = null,
                ReleaseDate = new DateTime(1967, 12, 29, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Good, the Bad and the Ugly",
                Year = 1966
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 96,
                MPAARating = null,
                ReleaseDate = new DateTime(1957, 4, 10, 0, 0, 0, DateTimeKind.Utc),
                Title = "12 Angry Men",
                Year = 1957
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 152,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2008, 07, 18, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Dark Knight",
                Year = 2008
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 195,
                MPAARating = "R",
                ReleaseDate = new DateTime(1993, 12, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "Schindler's List",
                Year = 1993
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 201,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2003, 12, 17, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Lord of the Rings: The Return of the King",
                Year = 2003
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 139,
                MPAARating = "R",
                ReleaseDate = new DateTime(1999, 10, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "Fight Club",
                Year = 1999
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 127,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1980, 05, 21, 0, 0, 0, DateTimeKind.Utc),
                Title = "Star Wars: Episode V - The Empire Strikes Back",
                Year = 1980
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 133,
                MPAARating = null,
                ReleaseDate = new DateTime(1975, 11, 21, 0, 0, 0, DateTimeKind.Utc),
                Title = "One Flew Over the Cuckoo's Nest",
                Year = 1975
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 178,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2001, 12, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Lord of the Rings: The Fellowship of the Ring",
                Year = 2001
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 148,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2010, 07, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "Inception",
                Year = 2010
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 146,
                MPAARating = "R",
                ReleaseDate = new DateTime(1990, 09, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Goodfellas",
                Year = 1990
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 121,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1977, 05, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "Star Wars",
                Year = 1977
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 141,
                MPAARating = null,
                ReleaseDate = new DateTime(1956, 11, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Seven Samurai",
                Year = 1954
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 136,
                MPAARating = "R",
                ReleaseDate = new DateTime(1999, 03, 31, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Matrix",
                Year = 1999
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 142,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(1994, 07, 06, 0, 0, 0, DateTimeKind.Utc),
                Title = "Forrest Gump",
                Year = 1994
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 130,
                MPAARating = "R",
                ReleaseDate = new DateTime(2002, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                Title = "City of God",
                Year = 2002
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 179,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2002, 12, 18, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Lord of the Rings: The Two Towers",
                Year = 2002
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 175,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(1968, 12, 21, 0, 0, 0, DateTimeKind.Utc),
                Title = "Once Upon a Time in the West",
                Year = 1968
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 127,
                MPAARating = "R",
                ReleaseDate = new DateTime(1995, 09, 22, 0, 0, 0, DateTimeKind.Utc),
                Title = "Se7en",
                Year = 1995
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 118,
                MPAARating = "R",
                ReleaseDate = new DateTime(1991, 02, 14, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Silence of the Lambs",
                Year = 1991
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 102,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1943, 01, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "Casablanca",
                Year = 1942
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 106,
                MPAARating = "R",
                ReleaseDate = new DateTime(1995, 08, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Usual Suspects",
                Year = 1995
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 115,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1981, 06, 12, 0, 0, 0, DateTimeKind.Utc),
                Title = "Raiders of the Lost Ark",
                Year = 1981
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 112,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1955, 01, 13, 0, 0, 0, DateTimeKind.Utc),
                Title = "Rear Window",
                Year = 1954
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 109,
                MPAARating = "TV-14",
                ReleaseDate = new DateTime(1960, 9, 8, 0, 0, 0, DateTimeKind.Utc),
                Title = "Psycho",
                Year = 1960
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 130,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1947, 01, 06, 0, 0, 0, DateTimeKind.Utc),
                Title = "It's a Wonderful Life",
                Year = 1946
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 110,
                MPAARating = "R",
                ReleaseDate = new DateTime(1994, 11, 18, 0, 0, 0, DateTimeKind.Utc),
                Title = "Léon: The Professional",
                Year = 1994
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 110,
                MPAARating = null,
                ReleaseDate = new DateTime(1950, 08, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "Sunset Blvd.",
                Year = 1950
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 113,
                MPAARating = "R",
                ReleaseDate = new DateTime(2000, 10, 11, 0, 0, 0, DateTimeKind.Utc),
                Title = "Memento",
                Year = 2000
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 165,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2012, 07, 20, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Dark Knight Rises",
                Year = 2012
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 119,
                MPAARating = "R",
                ReleaseDate = new DateTime(1999, 02, 12, 0, 0, 0, DateTimeKind.Utc),
                Title = "American History X",
                Year = 1998
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 153,
                MPAARating = "R",
                ReleaseDate = new DateTime(1979, 08, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "Apocalypse Now",
                Year = 1979
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 152,
                MPAARating = "R",
                ReleaseDate = new DateTime(1991, 07, 03, 0, 0, 0, DateTimeKind.Utc),
                Title = "Terminator 2: Judgment Day",
                Year = 1991
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 95,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1964, 01, 29, 0, 0, 0, DateTimeKind.Utc),
                Title = "Dr. Strangelove or: How I Learned to Stop Worrying and Love the Bomb",
                Year = 1964
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 169,
                MPAARating = "R",
                ReleaseDate = new DateTime(1998, 07, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "Saving Private Ryan",
                Year = 1998
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 117,
                MPAARating = "TV-14",
                ReleaseDate = new DateTime(1979, 05, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "Alien",
                Year = 1979
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 136,
                MPAARating = null,
                ReleaseDate = new DateTime(1959, 09, 26, 0, 0, 0, DateTimeKind.Utc),
                Title = "North by Northwest",
                Year = 1959
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 87,
                MPAARating = null,
                ReleaseDate = new DateTime(1931, 03, 07, 0, 0, 0, DateTimeKind.Utc),
                Title = "City Lights",
                Year = 1931
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 125,
                MPAARating = "PG",
                ReleaseDate = new DateTime(2001, 07, 20, 0, 0, 0, DateTimeKind.Utc),
                Title = "Spirited Away",
                Year = 2001
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 119,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1941, 9, 5, 0, 0, 0, DateTimeKind.Utc),
                Title = "Citizen Kane",
                Year = 1941
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 87,
                MPAARating = null,
                ReleaseDate = new DateTime(1936, 02, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "Modern Times",
                Year = 1936
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 142,
                MPAARating = "R",
                ReleaseDate = new DateTime(1980, 05, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Shining",
                Year = 1980
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 129,
                MPAARating = null,
                ReleaseDate = new DateTime(1958, 07, 21, 0, 0, 0, DateTimeKind.Utc),
                Title = "Vertigo",
                Year = 1958
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 116,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1985, 07, 03, 0, 0, 0, DateTimeKind.Utc),
                Title = "Back to the Future",
                Year = 1985
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 122,
                MPAARating = "R",
                ReleaseDate = new DateTime(1999, 10, 01, 0, 0, 0, DateTimeKind.Utc),
                Title = "American Beauty",
                Year = 1999
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 117,
                MPAARating = null,
                ReleaseDate = new DateTime(1931, 08, 30, 0, 0, 0, DateTimeKind.Utc),
                Title = "M",
                Year = 1931
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 150,
                MPAARating = "R",
                ReleaseDate = new DateTime(2003, 03, 28, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Pianist",
                Year = 2002
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 151,
                MPAARating = "R",
                ReleaseDate = new DateTime(2006, 10, 06, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Departed",
                Year = 2006
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 113,
                MPAARating = "R",
                ReleaseDate = new DateTime(1976, 02, 08, 0, 0, 0, DateTimeKind.Utc),
                Title = "Taxi Driver",
                Year = 1976
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 103,
                MPAARating = "G",
                ReleaseDate = new DateTime(2010, 06, 18, 0, 0, 0, DateTimeKind.Utc),
                Title = "Toy Story 3",
                Year = 2010
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 88,
                MPAARating = null,
                ReleaseDate = new DateTime(1957, 10, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "Paths of Glory",
                Year = 1957
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 118,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(1999, 02, 12, 0, 0, 0, DateTimeKind.Utc),
                Title = "Life Is Beautiful",
                Year = 1997
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 107,
                MPAARating = null,
                ReleaseDate = new DateTime(1944, 04, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "Double Indemnity",
                Year = 1944
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 154,
                MPAARating = "R",
                ReleaseDate = new DateTime(1986, 07, 18, 0, 0, 0, DateTimeKind.Utc),
                Title = "Aliens",
                Year = 1986
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 98,
                MPAARating = "G",
                ReleaseDate = new DateTime(2008, 06, 27, 0, 0, 0, DateTimeKind.Utc),
                Title = "WALL-E",
                Year = 2008
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 137,
                MPAARating = "R",
                ReleaseDate = new DateTime(2006, 03, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Lives of Others",
                Year = 2006
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 136,
                MPAARating = "R",
                ReleaseDate = new DateTime(1972, 02, 02, 0, 0, 0, DateTimeKind.Utc),
                Title = "A Clockwork Orange",
                Year = 1971
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 122,
                MPAARating = "R",
                ReleaseDate = new DateTime(2001, 04, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "Amélie",
                Year = 2001
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 155,
                MPAARating = "R",
                ReleaseDate = new DateTime(2000, 05, 05, 0, 0, 0, DateTimeKind.Utc),
                Title = "Gladiator",
                Year = 2000
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 189,
                MPAARating = "R",
                ReleaseDate = new DateTime(1999, 12, 10, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Green Mile",
                Year = 1999
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 112,
                MPAARating = "R",
                ReleaseDate = new DateTime(2011, 11, 02, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Intouchables",
                Year = 2011
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 227,
                MPAARating = null,
                ReleaseDate = new DateTime(1963, 01, 30, 0, 0, 0, DateTimeKind.Utc),
                Title = "Lawrence of Arabia",
                Year = 1962
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 129,
                MPAARating = null,
                ReleaseDate = new DateTime(1963, 03, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "To Kill a Mockingbird",
                Year = 1962
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 130,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2006, 10, 20, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Prestige",
                Year = 2006
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 125,
                MPAARating = null,
                ReleaseDate = new DateTime(1941, 3, 7, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Great Dictator",
                Year = 1940
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 99,
                MPAARating = "R",
                ReleaseDate = new DateTime(1992, 10, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "Reservoir Dogs",
                Year = 1992
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 149,
                MPAARating = "R",
                ReleaseDate = new DateTime(1982, 02, 10, 0, 0, 0, DateTimeKind.Utc),
                Title = "Das Boot",
                Year = 1981
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 102,
                MPAARating = "NC-17",
                ReleaseDate = new DateTime(2000, 10, 27, 0, 0, 0, DateTimeKind.Utc),
                Title = "Requiem for a Dream",
                Year = 2000
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 93,
                MPAARating = null,
                ReleaseDate = new DateTime(1949, 08, 31, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Third Man",
                Year = 1949
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 126,
                MPAARating = null,
                ReleaseDate = new DateTime(1948, 01, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Treasure of the Sierra Madre",
                Year = 1948
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 108,
                MPAARating = "R",
                ReleaseDate = new DateTime(2004, 03, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Eternal Sunshine of the Spotless Mind",
                Year = 2004
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 155,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1990, 02, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "Cinema Paradiso",
                Year = 1988
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 139,
                MPAARating = "R",
                ReleaseDate = new DateTime(1984, 05, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "Once Upon a Time in America",
                Year = 1984
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 130,
                MPAARating = null,
                ReleaseDate = new DateTime(1974, 06, 20, 0, 0, 0, DateTimeKind.Utc),
                Title = "Chinatown",
                Year = 1974
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 138,
                MPAARating = "R",
                ReleaseDate = new DateTime(1997, 09, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "L.A. Confidential",
                Year = 1997
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 89,
                MPAARating = "G",
                ReleaseDate = new DateTime(1994, 06, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Lion King",
                Year = 1994
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 134,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1983, 05, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "Star Wars: Episode VI - Return of the Jedi",
                Year = 1983
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 116,
                MPAARating = "R",
                ReleaseDate = new DateTime(1987, 06, 26, 0, 0, 0, DateTimeKind.Utc),
                Title = "Full Metal Jacket",
                Year = 1987
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 91,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1975, 05, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "Monty Python and the Holy Grail",
                Year = 1975
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 177,
                MPAARating = "R",
                ReleaseDate = new DateTime(1995, 05, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "Braveheart",
                Year = 1995
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 103,
                MPAARating = null,
                ReleaseDate = new DateTime(1952, 04, 11, 0, 0, 0, DateTimeKind.Utc),
                Title = "Singin' in the Rain",
                Year = 1952
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 120,
                MPAARating = "R",
                ReleaseDate = new DateTime(2003, 11, 21, 0, 0, 0, DateTimeKind.Utc),
                Title = "Oldboy",
                Year = 2003
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 120,
                MPAARating = null,
                ReleaseDate = new DateTime(1959, 03, 29, 0, 0, 0, DateTimeKind.Utc),
                Title = "Some Like It Hot",
                Year = 1959
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 160,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1984, 09, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Amadeus",
                Year = 1984
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 114,
                MPAARating = null,
                ReleaseDate = new DateTime(1927, 03, 13, 0, 0, 0, DateTimeKind.Utc),
                Title = "Metropolis",
                Year = 1927
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 88,
                MPAARating = null,
                ReleaseDate = new DateTime(1951, 12, 26, 0, 0, 0, DateTimeKind.Utc),
                Title = "Rashomon",
                Year = 1950
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 93,
                MPAARating = null,
                ReleaseDate = new DateTime(1949, 12, 13, 0, 0, 0, DateTimeKind.Utc),
                Title = "Bicycle Thieves",
                Year = 1948
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 141,
                MPAARating = null,
                ReleaseDate = new DateTime(1968, 4, 6, 0, 0, 0, DateTimeKind.Utc),
                Title = "2001: A Space Odyssey",
                Year = 1968
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 131,
                MPAARating = "R",
                ReleaseDate = new DateTime(1992, 08, 07, 0, 0, 0, DateTimeKind.Utc),
                Title = "Unforgiven",
                Year = 1992
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 138,
                MPAARating = null,
                ReleaseDate = new DateTime(1951, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "All About Eve",
                Year = 1950
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 125,
                MPAARating = null,
                ReleaseDate = new DateTime(1960, 9, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Apartment",
                Year = 1960
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 127,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1989, 05, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "Indiana Jones and the Last Crusade",
                Year = 1989
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 129,
                MPAARating = null,
                ReleaseDate = new DateTime(1974, 01, 10, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Sting",
                Year = 1973
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 129,
                MPAARating = "R",
                ReleaseDate = new DateTime(1980, 12, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Raging Bull",
                Year = 1980
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 161,
                MPAARating = null,
                ReleaseDate = new DateTime(1957, 12, 14, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Bridge on the River Kwai",
                Year = 1957
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 131,
                MPAARating = "R",
                ReleaseDate = new DateTime(1988, 07, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "Die Hard",
                Year = 1988
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 116,
                MPAARating = null,
                ReleaseDate = new DateTime(1958, 2, 6, 0, 0, 0, DateTimeKind.Utc),
                Title = "Witness for the Prosecution",
                Year = 1957
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 140,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2005, 06, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "Batman Begins",
                Year = 2005
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 123,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2011, 03, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "A Separation",
                Year = 2011
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 89,
                MPAARating = null,
                ReleaseDate = new DateTime(1988, 04, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "Grave of the Fireflies",
                Year = 1988
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 118,
                MPAARating = "R",
                ReleaseDate = new DateTime(2007, 01, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Pan's Labyrinth",
                Year = 2006
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 156,
                MPAARating = "R",
                ReleaseDate = new DateTime(2004, 09, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "Downfall",
                Year = 2004
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 129,
                MPAARating = null,
                ReleaseDate = new DateTime(1939, 10, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Mr. Smith Goes to Washington",
                Year = 1939
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 75,
                MPAARating = "TV-MA",
                ReleaseDate = new DateTime(1961, 09, 13, 0, 0, 0, DateTimeKind.Utc),
                Title = "Yojimbo",
                Year = 1961
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 172,
                MPAARating = null,
                ReleaseDate = new DateTime(1963, 7, 4, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Great Escape",
                Year = 1963
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 132,
                MPAARating = "R",
                ReleaseDate = new DateTime(1967, 5, 10, 0, 0, 0, DateTimeKind.Utc),
                Title = "For a Few Dollars More",
                Year = 1965
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 102,
                MPAARating = "R",
                ReleaseDate = new DateTime(2001, 01, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Snatch.",
                Year = 2000
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 153,
                MPAARating = "R",
                ReleaseDate = new DateTime(2009, 08, 21, 0, 0, 0, DateTimeKind.Utc),
                Title = "Inglourious Basterds",
                Year = 2009
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 108,
                MPAARating = null,
                ReleaseDate = new DateTime(1954, 06, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "On the Waterfront",
                Year = 1954
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 124,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1980, 10, 10, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Elephant Man",
                Year = 1980
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 96,
                MPAARating = null,
                ReleaseDate = new DateTime(1958, 10, 13, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Seventh Seal",
                Year = 1957
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 81,
                MPAARating = "TV-G",
                ReleaseDate = new DateTime(1995, 11, 22, 0, 0, 0, DateTimeKind.Utc),
                Title = "Toy Story",
                Year = 1995
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 100,
                MPAARating = null,
                ReleaseDate = new DateTime(1941, 10, 18, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Maltese Falcon",
                Year = 1941
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 170,
                MPAARating = "R",
                ReleaseDate = new DateTime(1995, 12, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "Heat",
                Year = 1995
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 75,
                MPAARating = null,
                ReleaseDate = new DateTime(1927, 02, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "The General",
                Year = 1926
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 116,
                MPAARating = "R",
                ReleaseDate = new DateTime(2009, 01, 09, 0, 0, 0, DateTimeKind.Utc),
                Title = "Gran Torino",
                Year = 2008
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 130,
                MPAARating = null,
                ReleaseDate = new DateTime(1940, 04, 12, 0, 0, 0, DateTimeKind.Utc),
                Title = "Rebecca",
                Year = 1940
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 117,
                MPAARating = "R",
                ReleaseDate = new DateTime(1982, 06, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "Blade Runner",
                Year = 1982
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 143,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2012, 05, 04, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Avengers",
                Year = 2012
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 91,
                MPAARating = null,
                ReleaseDate = new DateTime(1959, 06, 22, 0, 0, 0, DateTimeKind.Utc),
                Title = "Wild Strawberries",
                Year = 1957
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 98,
                MPAARating = "R",
                ReleaseDate = new DateTime(1996, 04, 05, 0, 0, 0, DateTimeKind.Utc),
                Title = "Fargo",
                Year = 1996
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 68,
                MPAARating = null,
                ReleaseDate = new DateTime(1921, 2, 6, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Kid",
                Year = 1921
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 170,
                MPAARating = "R",
                ReleaseDate = new DateTime(1983, 12, 09, 0, 0, 0, DateTimeKind.Utc),
                Title = "Scarface",
                Year = 1983
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 108,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(1958, 6, 8, 0, 0, 0, DateTimeKind.Utc),
                Title = "Touch of Evil",
                Year = 1958
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 117,
                MPAARating = "R",
                ReleaseDate = new DateTime(1998, 03, 06, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Big Lebowski",
                Year = 1998
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 162,
                MPAARating = "R",
                ReleaseDate = new DateTime(1985, 06, 01, 0, 0, 0, DateTimeKind.Utc),
                Title = "Ran",
                Year = 1985
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 182,
                MPAARating = "R",
                ReleaseDate = new DateTime(1979, 02, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Deer Hunter",
                Year = 1978
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 126,
                MPAARating = null,
                ReleaseDate = new DateTime(1967, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                Title = "Cool Hand Luke",
                Year = 1967
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 147,
                MPAARating = "R",
                ReleaseDate = new DateTime(2005, 04, 01, 0, 0, 0, DateTimeKind.Utc),
                Title = "Sin City",
                Year = 2005
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 72,
                MPAARating = null,
                ReleaseDate = new DateTime(1925, 6, 26, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Gold Rush",
                Year = 1925
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 101,
                MPAARating = null,
                ReleaseDate = new DateTime(1951, 06, 30, 0, 0, 0, DateTimeKind.Utc),
                Title = "Strangers on a Train",
                Year = 1951
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 105,
                MPAARating = null,
                ReleaseDate = new DateTime(1934, 02, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "It Happened One Night",
                Year = 1934
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 122,
                MPAARating = "R",
                ReleaseDate = new DateTime(2007, 11, 21, 0, 0, 0, DateTimeKind.Utc),
                Title = "No Country for Old Men",
                Year = 2007
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 130,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1975, 06, 20, 0, 0, 0, DateTimeKind.Utc),
                Title = "Jaws",
                Year = 1975
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 107,
                MPAARating = "R",
                ReleaseDate = new DateTime(1999, 03, 05, 0, 0, 0, DateTimeKind.Utc),
                Title = "Lock, Stock and Two Smoking Barrels",
                Year = 1998
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 107,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(1999, 08, 06, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Sixth Sense",
                Year = 1999
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 121,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2005, 02, 04, 0, 0, 0, DateTimeKind.Utc),
                Title = "Hotel Rwanda",
                Year = 2004
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 85,
                MPAARating = null,
                ReleaseDate = new DateTime(1952, 07, 30, 0, 0, 0, DateTimeKind.Utc),
                Title = "High Noon",
                Year = 1952
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 120,
                MPAARating = "R",
                ReleaseDate = new DateTime(1986, 12, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "Platoon",
                Year = 1986
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 109,
                MPAARating = "R",
                ReleaseDate = new DateTime(1982, 06, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Thing",
                Year = 1982
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 110,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1969, 10, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "Butch Cassidy and the Sundance Kid",
                Year = 1969
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 101,
                MPAARating = null,
                ReleaseDate = new DateTime(1939, 08, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Wizard of Oz",
                Year = 1939
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 178,
                MPAARating = "R",
                ReleaseDate = new DateTime(1995, 11, 22, 0, 0, 0, DateTimeKind.Utc),
                Title = "Casino",
                Year = 1995
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 94,
                MPAARating = "R",
                ReleaseDate = new DateTime(1996, 07, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Trainspotting",
                Year = 1996
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 111,
                MPAARating = "TV-14",
                ReleaseDate = new DateTime(2003, 10, 10, 0, 0, 0, DateTimeKind.Utc),
                Title = "Kill Bill: Vol. 1",
                Year = 2003
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 140,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2011, 09, 09, 0, 0, 0, DateTimeKind.Utc),
                Title = "Warrior",
                Year = 2011
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 93,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1977, 04, 20, 0, 0, 0, DateTimeKind.Utc),
                Title = "Annie Hall",
                Year = 1977
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 101,
                MPAARating = null,
                ReleaseDate = new DateTime(1946, 9, 6, 0, 0, 0, DateTimeKind.Utc),
                Title = "Notorious",
                Year = 1946
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 129,
                MPAARating = "R",
                ReleaseDate = new DateTime(2009, 08, 13, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Secret in Their Eyes",
                Year = 2009
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 238,
                MPAARating = "G",
                ReleaseDate = new DateTime(1940, 01, 17, 0, 0, 0, DateTimeKind.Utc),
                Title = "Gone with the Wind",
                Year = 1939
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 126,
                MPAARating = "R",
                ReleaseDate = new DateTime(1998, 01, 09, 0, 0, 0, DateTimeKind.Utc),
                Title = "Good Will Hunting",
                Year = 1997
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 118,
                MPAARating = "R",
                ReleaseDate = new DateTime(2010, 12, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "The King's Speech",
                Year = 2010
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 129,
                MPAARating = null,
                ReleaseDate = new DateTime(1940, 03, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Grapes of Wrath",
                Year = 1940
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 148,
                MPAARating = "R",
                ReleaseDate = new DateTime(2007, 09, 21, 0, 0, 0, DateTimeKind.Utc),
                Title = "Into the Wild",
                Year = 2007
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 94,
                MPAARating = "R",
                ReleaseDate = new DateTime(1979, 08, 17, 0, 0, 0, DateTimeKind.Utc),
                Title = "Life of Brian",
                Year = 1979
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 100,
                MPAARating = "G",
                ReleaseDate = new DateTime(2003, 05, 30, 0, 0, 0, DateTimeKind.Utc),
                Title = "Finding Nemo",
                Year = 2003
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 132,
                MPAARating = "R",
                ReleaseDate = new DateTime(2006, 03, 17, 0, 0, 0, DateTimeKind.Utc),
                Title = "V for Vendetta",
                Year = 2005
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 98,
                MPAARating = "PG",
                ReleaseDate = new DateTime(2010, 03, 26, 0, 0, 0, DateTimeKind.Utc),
                Title = "How to Train Your Dragon",
                Year = 2010
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 86,
                MPAARating = "G",
                ReleaseDate = new DateTime(1988, 04, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "My Neighbor Totoro",
                Year = 1988
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 114,
                MPAARating = null,
                ReleaseDate = new DateTime(1946, 08, 31, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Big Sleep",
                Year = 1946
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 105,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1954, 05, 29, 0, 0, 0, DateTimeKind.Utc),
                Title = "Dial M for Murder",
                Year = 1954
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 212,
                MPAARating = null,
                ReleaseDate = new DateTime(1960, 03, 30, 0, 0, 0, DateTimeKind.Utc),
                Title = "Ben-Hur",
                Year = 1959
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 107,
                MPAARating = "R",
                ReleaseDate = new DateTime(1984, 10, 26, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Terminator",
                Year = 1984
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 121,
                MPAARating = "R",
                ReleaseDate = new DateTime(1976, 11, 27, 0, 0, 0, DateTimeKind.Utc),
                Title = "Network",
                Year = 1976
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 132,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2005, 01, 28, 0, 0, 0, DateTimeKind.Utc),
                Title = "Million Dollar Baby",
                Year = 2004
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 108,
                MPAARating = "R",
                ReleaseDate = new DateTime(2010, 12, 17, 0, 0, 0, DateTimeKind.Utc),
                Title = "Black Swan",
                Year = 2010
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 93,
                MPAARating = null,
                ReleaseDate = new DateTime(1955, 11, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Night of the Hunter",
                Year = 1955
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 158,
                MPAARating = "R",
                ReleaseDate = new DateTime(2008, 01, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "There Will Be Blood",
                Year = 2007
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 89,
                MPAARating = "R",
                ReleaseDate = new DateTime(1986, 08, 08, 0, 0, 0, DateTimeKind.Utc),
                Title = "Stand by Me",
                Year = 1986
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 113,
                MPAARating = "R",
                ReleaseDate = new DateTime(2002, 01, 30, 0, 0, 0, DateTimeKind.Utc),
                Title = "Donnie Darko",
                Year = 2001
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 101,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1993, 02, 12, 0, 0, 0, DateTimeKind.Utc),
                Title = "Groundhog Day",
                Year = 1993
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 125,
                MPAARating = "R",
                ReleaseDate = new DateTime(1975, 09, 21, 0, 0, 0, DateTimeKind.Utc),
                Title = "Dog Day Afternoon",
                Year = 1975
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 129,
                MPAARating = "R",
                ReleaseDate = new DateTime(1996, 01, 05, 0, 0, 0, DateTimeKind.Utc),
                Title = "Twelve Monkeys",
                Year = 1995
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 154,
                MPAARating = "R",
                ReleaseDate = new DateTime(2000, 06, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "Amores Perros",
                Year = 2000
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 115,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2007, 08, 03, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Bourne Ultimatum",
                Year = 2007
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 92,
                MPAARating = null,
                ReleaseDate = new DateTime(2009, 04, 09, 0, 0, 0, DateTimeKind.Utc),
                Title = "Mary and Max",
                Year = 2009
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 99,
                MPAARating = null,
                ReleaseDate = new DateTime(1959, 11, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "The 400 Blows",
                Year = 1959
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 83,
                MPAARating = null,
                ReleaseDate = new DateTime(1967, 03, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "Persona",
                Year = 1966
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 106,
                MPAARating = null,
                ReleaseDate = new DateTime(1967, 12, 22, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Graduate",
                Year = 1967
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 191,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1983, 02, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "Gandhi",
                Year = 1982
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 85,
                MPAARating = null,
                ReleaseDate = new DateTime(1956, 6, 6, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Killing",
                Year = 1956
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 119,
                MPAARating = "PG",
                ReleaseDate = new DateTime(2005, 06, 17, 0, 0, 0, DateTimeKind.Utc),
                Title = "Howl's Moving Castle",
                Year = 2004
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 100,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2012, 01, 20, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Artist",
                Year = 2011
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 98,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1987, 09, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Princess Bride",
                Year = 1987
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 120,
                MPAARating = "R",
                ReleaseDate = new DateTime(2012, 10, 12, 0, 0, 0, DateTimeKind.Utc),
                Title = "Argo",
                Year = 2012
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 120,
                MPAARating = "R",
                ReleaseDate = new DateTime(2009, 01, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "Slumdog Millionaire",
                Year = 2008
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 131,
                MPAARating = null,
                ReleaseDate = new DateTime(1966, 06, 22, 0, 0, 0, DateTimeKind.Utc),
                Title = "Who's Afraid of Virginia Woolf?",
                Year = 1966
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 108,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1956, 07, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "La Strada",
                Year = 1954
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 126,
                MPAARating = null,
                ReleaseDate = new DateTime(1962, 10, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Manchurian Candidate",
                Year = 1962
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 134,
                MPAARating = null,
                ReleaseDate = new DateTime(1961, 09, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Hustler",
                Year = 1961
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 135,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2002, 01, 04, 0, 0, 0, DateTimeKind.Utc),
                Title = "A Beautiful Mind",
                Year = 2001
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 145,
                MPAARating = "R",
                ReleaseDate = new DateTime(1969, 06, 18, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Wild Bunch",
                Year = 1969
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 119,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1976, 12, 03, 0, 0, 0, DateTimeKind.Utc),
                Title = "Rocky",
                Year = 1976
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 160,
                MPAARating = "TV-PG",
                ReleaseDate = new DateTime(1959, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                Title = "Anatomy of a Murder",
                Year = 1959
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 120,
                MPAARating = null,
                ReleaseDate = new DateTime(1953, 8, 10, 0, 0, 0, DateTimeKind.Utc),
                Title = "Stalag 17",
                Year = 1953
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 122,
                MPAARating = "R",
                ReleaseDate = new DateTime(1974, 03, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Exorcist",
                Year = 1973
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 138,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1972, 12, 10, 0, 0, 0, DateTimeKind.Utc),
                Title = "Sleuth",
                Year = 1972
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 80,
                MPAARating = null,
                ReleaseDate = new DateTime(1948, 8, 28, 0, 0, 0, DateTimeKind.Utc),
                Title = "Rope",
                Year = 1948
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 184,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1975, 12, 18, 0, 0, 0, DateTimeKind.Utc),
                Title = "Barry Lyndon",
                Year = 1975
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 123,
                MPAARating = null,
                ReleaseDate = new DateTime(1962, 4, 22, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Man Who Shot Liberty Valance",
                Year = 1962
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 112,
                MPAARating = "R",
                ReleaseDate = new DateTime(2009, 08, 14, 0, 0, 0, DateTimeKind.Utc),
                Title = "District 9",
                Year = 2009
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 163,
                MPAARating = null,
                ReleaseDate = new DateTime(1980, 04, 17, 0, 0, 0, DateTimeKind.Utc),
                Title = "Stalker",
                Year = 1979
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 101,
                MPAARating = "R",
                ReleaseDate = new DateTime(2002, 12, 12, 0, 0, 0, DateTimeKind.Utc),
                Title = "Infernal Affairs",
                Year = 2002
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 118,
                MPAARating = null,
                ReleaseDate = new DateTime(1953, 9, 2, 0, 0, 0, DateTimeKind.Utc),
                Title = "Roman Holiday",
                Year = 1953
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 103,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1998, 06, 05, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Truman Show",
                Year = 1998
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 111,
                MPAARating = "G",
                ReleaseDate = new DateTime(2007, 06, 29, 0, 0, 0, DateTimeKind.Utc),
                Title = "Ratatouille",
                Year = 2007
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 143,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2003, 07, 09, 0, 0, 0, DateTimeKind.Utc),
                Title = "Pirates of the Caribbean: The Curse of the Black Pearl",
                Year = 2003
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 106,
                MPAARating = "R",
                ReleaseDate = new DateTime(2008, 12, 12, 0, 0, 0, DateTimeKind.Utc),
                Title = "Ip Man",
                Year = 2008
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 112,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2007, 05, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Diving Bell and the Butterfly",
                Year = 2007
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 130,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2011, 07, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "Harry Potter and the Deathly Hallows: Part 2",
                Year = 2011
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 99,
                MPAARating = "R",
                ReleaseDate = new DateTime(1967, 01, 18, 0, 0, 0, DateTimeKind.Utc),
                Title = "A Fistful of Dollars",
                Year = 1964
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 125,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1951, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                Title = "A Streetcar Named Desire",
                Year = 1951
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 92,
                MPAARating = "G",
                ReleaseDate = new DateTime(2001, 11, 02, 0, 0, 0, DateTimeKind.Utc),
                Title = "Monsters, Inc.",
                Year = 2001
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 133,
                MPAARating = "R",
                ReleaseDate = new DateTime(1994, 02, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "In the Name of the Father",
                Year = 1993
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 127,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2009, 05, 08, 0, 0, 0, DateTimeKind.Utc),
                Title = "Star Trek",
                Year = 2009
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 84,
                MPAARating = "G",
                ReleaseDate = new DateTime(1991, 11, 22, 0, 0, 0, DateTimeKind.Utc),
                Title = "Beauty and the Beast",
                Year = 1991
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 136,
                MPAARating = "R",
                ReleaseDate = new DateTime(1968, 06, 12, 0, 0, 0, DateTimeKind.Utc),
                Title = "Rosemary's Baby",
                Year = 1968
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 104,
                MPAARating = null,
                ReleaseDate = new DateTime(1950, 10, 13, 0, 0, 0, DateTimeKind.Utc),
                Title = "Harvey",
                Year = 1950
            },
            new Movie {
                BestPictureWinner = false,
                Duration = 117,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1984, 3, 11, 0, 0, 0, DateTimeKind.Utc),
                Title = "Nauticaä of the Valley of the Wind",
                Year = 1984
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 109,
                MPAARating = "R",
                ReleaseDate = new DateTime(2009, 01, 30, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Wrestler",
                Year = 2008
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 133,
                MPAARating = null,
                ReleaseDate = new DateTime(1930, 08, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "All Quiet on the Western Front",
                Year = 1930
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 98,
                MPAARating = null,
                ReleaseDate = new DateTime(1996, 02, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "La Haine",
                Year = 1995
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 133,
                MPAARating = "R",
                ReleaseDate = new DateTime(1988, 12, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "Rain Man",
                Year = 1988
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 66,
                MPAARating = null,
                ReleaseDate = new DateTime(1925, 12, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "Battleship Potemkin",
                Year = 1925
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 138,
                MPAARating = "R",
                ReleaseDate = new DateTime(2010, 02, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Shutter Island",
                Year = 2010
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 81,
                MPAARating = null,
                ReleaseDate = new DateTime(1929, 6, 3, 0, 0, 0, DateTimeKind.Utc),
                Title = "Nosferatu",
                Year = 1922
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 103,
                MPAARating = "R",
                ReleaseDate = new DateTime(2003, 09, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Spring, Summer, Fall, Winter... and Spring",
                Year = 2003
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 96,
                MPAARating = "R",
                ReleaseDate = new DateTime(1979, 04, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "Manhattan",
                Year = 1979
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 138,
                MPAARating = "R",
                ReleaseDate = new DateTime(2003, 10, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "Mystic River",
                Year = 2003
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 102,
                MPAARating = null,
                ReleaseDate = new DateTime(1938, 2, 18, 0, 0, 0, DateTimeKind.Utc),
                Title = "Bringing Up Baby",
                Year = 1938
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 108,
                MPAARating = null,
                ReleaseDate = new DateTime(1943, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "Shadow of a Doubt",
                Year = 1943
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 125,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2004, 01, 09, 0, 0, 0, DateTimeKind.Utc),
                Title = "Big Fish",
                Year = 2003
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 124,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1986, 08, 02, 0, 0, 0, DateTimeKind.Utc),
                Title = "Castle in the Sky",
                Year = 1986
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 151,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1973, 12, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "Papillon",
                Year = 1973
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 76,
                MPAARating = "PG",
                ReleaseDate = new DateTime(1993, 10, 29, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Nightmare Before Christmas",
                Year = 1993
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 119,
                MPAARating = "R",
                ReleaseDate = new DateTime(1987, 06, 03, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Untouchables",
                Year = 1987
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 127,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(1993, 06, 11, 0, 0, 0, DateTimeKind.Utc),
                Title = "Jurassic Park",
                Year = 1993
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 115,
                MPAARating = "R",
                ReleaseDate = new DateTime(2008, 10, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "Let the Right One In",
                Year = 2008
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 109,
                MPAARating = null,
                ReleaseDate = new DateTime(1967, 10, 14, 0, 0, 0, DateTimeKind.Utc),
                Title = "In the Heat of the Night",
                Year = 1967
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 170,
                MPAARating = "PG-13",
                ReleaseDate = new DateTime(2009, 12, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "3 Idiots",
                Year = 2009
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 118,
                MPAARating = null,
                ReleaseDate = new DateTime(1944, 9, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "Arsenic and Old Lace",
                Year = 1944
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 119,
                MPAARating = null,
                ReleaseDate = new DateTime(1956, 3, 13, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Searchers",
                Year = 1956
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 98,
                MPAARating = "PG",
                ReleaseDate = new DateTime(2000, 09, 29, 0, 0, 0, DateTimeKind.Utc),
                Title = "In the Mood for Love",
                Year = 2000
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 141,
                MPAARating = null,
                ReleaseDate = new DateTime(1959, 4, 4, 0, 0, 0, DateTimeKind.Utc),
                Title = "Rio Bravo",
                Year = 1959
            },
        };
    }
}
