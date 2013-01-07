using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZumoE2ETestApp.Tests.Types;

namespace ZumoE2ETestApp.Tests
{
    public static class ZumoQueryTestData
    {
        public static Movie[] AllMovies = new Movie[]
        {
            new Movie
            {
                BestPictureWinner = false,
                Duration = 142,
                Rating = "R",
                ReleaseDate = new DateTime(1994, 10, 14, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Shawshank Redemption",
                Year = 1994
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 175,
                Rating = "R",
                ReleaseDate = new DateTime(1972, 3, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Godfather",
                Year = 1972
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 200,
                Rating = "R",
                ReleaseDate = new DateTime(1974, 12, 20, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Godfather: Part II",
                Year = 1974
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 168,
                Rating = "R",
                ReleaseDate = new DateTime(1994, 10, 14, 0, 0, 0, DateTimeKind.Utc),
                Title = "Pulp Fiction",
                Year = 1994
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 161,
                Rating = "Approved",
                ReleaseDate = new DateTime(1967, 12, 29, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Good, the Bad and the Ugly",
                Year = 1966
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 96,
                Rating = "TV-PG",
                ReleaseDate = new DateTime(1957, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                Title = "12 Angry Men",
                Year = 1957
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 152,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2008, 7, 18, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Dark Knight",
                Year = 2008
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 195,
                Rating = "R",
                ReleaseDate = new DateTime(1993, 12, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "Schindler's List",
                Year = 1993
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 201,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2003, 12, 17, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Lord of the Rings: The Return of the King",
                Year = 2003
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 139,
                Rating = "R",
                ReleaseDate = new DateTime(1999, 10, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "Fight Club",
                Year = 1999
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 127,
                Rating = "PG",
                ReleaseDate = new DateTime(1980, 5, 21, 0, 0, 0, DateTimeKind.Utc),
                Title = "Star Wars: Episode V - The Empire Strikes Back",
                Year = 1980
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 133,
                Rating = "R",
                ReleaseDate = new DateTime(1975, 11, 21, 0, 0, 0, DateTimeKind.Utc),
                Title = "One Flew Over the Cuckoo's Nest",
                Year = 1975
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 178,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2001, 12, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Lord of the Rings: The Fellowship of the Ring",
                Year = 2001
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 148,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2010, 7, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "Inception",
                Year = 2010
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 146,
                Rating = "R",
                ReleaseDate = new DateTime(1990, 9, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Goodfellas",
                Year = 1990
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 121,
                Rating = "PG",
                ReleaseDate = new DateTime(1977, 5, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "Star Wars",
                Year = 1977
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 141,
                Rating = "Unrated",
                ReleaseDate = new DateTime(1956, 11, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Seven Samurai",
                Year = 1954
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 136,
                Rating = "R",
                ReleaseDate = new DateTime(1999, 3, 31, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Matrix",
                Year = 1999
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 142,
                Rating = "PG-13",
                ReleaseDate = new DateTime(1994, 7, 6, 0, 0, 0, DateTimeKind.Utc),
                Title = "Forrest Gump",
                Year = 1994
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 130,
                Rating = "R",
                ReleaseDate = new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Title = "City of God",
                Year = 2002
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 179,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2002, 12, 18, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Lord of the Rings: The Two Towers",
                Year = 2002
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 175,
                Rating = "M",
                ReleaseDate = new DateTime(1968, 12, 21, 0, 0, 0, DateTimeKind.Utc),
                Title = "Once Upon a Time in the West",
                Year = 1968
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 127,
                Rating = "R",
                ReleaseDate = new DateTime(1995, 9, 22, 0, 0, 0, DateTimeKind.Utc),
                Title = "Se7en",
                Year = 1995
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 118,
                Rating = "R",
                ReleaseDate = new DateTime(1991, 2, 14, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Silence of the Lambs",
                Year = 1991
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 102,
                Rating = "Approved",
                ReleaseDate = new DateTime(1943, 1, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "Casablanca",
                Year = 1942
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 106,
                Rating = "R",
                ReleaseDate = new DateTime(1995, 8, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Usual Suspects",
                Year = 1995
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 115,
                Rating = "PG",
                ReleaseDate = new DateTime(1981, 6, 12, 0, 0, 0, DateTimeKind.Utc),
                Title = "Raiders of the Lost Ark",
                Year = 1981
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 112,
                Rating = "PG",
                ReleaseDate = new DateTime(1955, 1, 14, 0, 0, 0, DateTimeKind.Utc),
                Title = "Rear Window",
                Year = 1954
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 109,
                Rating = "TV-14",
                ReleaseDate = new DateTime(1960, 9, 8, 0, 0, 0, DateTimeKind.Utc),
                Title = "Psycho",
                Year = 1960
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 130,
                Rating = "Unrated",
                ReleaseDate = new DateTime(1947, 1, 7, 0, 0, 0, DateTimeKind.Utc),
                Title = "It's a Wonderful Life",
                Year = 1946
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 110,
                Rating = "R",
                ReleaseDate = new DateTime(1994, 11, 18, 0, 0, 0, DateTimeKind.Utc),
                Title = "L�on: The Professional",
                Year = 1994
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 110,
                Rating = "Passed",
                ReleaseDate = new DateTime(1950, 8, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "Sunset Blvd.",
                Year = 1950
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 113,
                Rating = "R",
                ReleaseDate = new DateTime(2000, 10, 11, 0, 0, 0, DateTimeKind.Utc),
                Title = "Memento",
                Year = 2000
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 165,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2012, 7, 20, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Dark Knight Rises",
                Year = 2012
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 119,
                Rating = "R",
                ReleaseDate = new DateTime(1999, 2, 12, 0, 0, 0, DateTimeKind.Utc),
                Title = "American History X",
                Year = 1998
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 153,
                Rating = "R",
                ReleaseDate = new DateTime(1979, 8, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "Apocalypse Now",
                Year = 1979
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 152,
                Rating = "R",
                ReleaseDate = new DateTime(1991, 7, 3, 0, 0, 0, DateTimeKind.Utc),
                Title = "Terminator 2: Judgment Day",
                Year = 1991
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 95,
                Rating = "TV-PG",
                ReleaseDate = new DateTime(1964, 1, 29, 0, 0, 0, DateTimeKind.Utc),
                Title = "Dr. Strangelove or: How I Learned to Stop Worrying and Love the Bomb",
                Year = 1964
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 169,
                Rating = "R",
                ReleaseDate = new DateTime(1998, 7, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "Saving Private Ryan",
                Year = 1998
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 117,
                Rating = "TV-14",
                ReleaseDate = new DateTime(1979, 5, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "Alien",
                Year = 1979
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 136,
                Rating = "Approved",
                ReleaseDate = new DateTime(1959, 9, 26, 0, 0, 0, DateTimeKind.Utc),
                Title = "North by Northwest",
                Year = 1959
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 87,
                Rating = "G",
                ReleaseDate = new DateTime(1931, 3, 7, 0, 0, 0, DateTimeKind.Utc),
                Title = "City Lights",
                Year = 1931
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 125,
                Rating = "PG",
                ReleaseDate = new DateTime(2001, 7, 20, 0, 0, 0, DateTimeKind.Utc),
                Title = "Spirited Away",
                Year = 2001
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 119,
                Rating = "PG",
                ReleaseDate = new DateTime(1941, 9, 5, 0, 0, 0, DateTimeKind.Utc),
                Title = "Citizen Kane",
                Year = 1941
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 87,
                Rating = "Approved",
                ReleaseDate = new DateTime(1936, 2, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "Modern Times",
                Year = 1936
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 142,
                Rating = "R",
                ReleaseDate = new DateTime(1980, 5, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Shining",
                Year = 1980
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 129,
                Rating = "Approved",
                ReleaseDate = new DateTime(1958, 7, 21, 0, 0, 0, DateTimeKind.Utc),
                Title = "Vertigo",
                Year = 1958
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 116,
                Rating = "PG",
                ReleaseDate = new DateTime(1985, 7, 3, 0, 0, 0, DateTimeKind.Utc),
                Title = "Back to the Future",
                Year = 1985
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 122,
                Rating = "R",
                ReleaseDate = new DateTime(1999, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                Title = "American Beauty",
                Year = 1999
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 150,
                Rating = "TV-MA",
                ReleaseDate = new DateTime(2003, 3, 28, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Pianist",
                Year = 2002
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 151,
                Rating = "R",
                ReleaseDate = new DateTime(2006, 10, 6, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Departed",
                Year = 2006
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 113,
                Rating = "R",
                ReleaseDate = new DateTime(1976, 2, 8, 0, 0, 0, DateTimeKind.Utc),
                Title = "Taxi Driver",
                Year = 1976
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 103,
                Rating = "G",
                ReleaseDate = new DateTime(2010, 6, 18, 0, 0, 0, DateTimeKind.Utc),
                Title = "Toy Story 3",
                Year = 2010
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 88,
                Rating = "TV-14",
                ReleaseDate = new DateTime(1957, 10, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "Paths of Glory",
                Year = 1957
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 118,
                Rating = "PG-13",
                ReleaseDate = new DateTime(1999, 2, 12, 0, 0, 0, DateTimeKind.Utc),
                Title = "Life Is Beautiful",
                Year = 1997
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 107,
                Rating = "Passed",
                ReleaseDate = new DateTime(1944, 4, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "Double Indemnity",
                Year = 1944
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 154,
                Rating = "R",
                ReleaseDate = new DateTime(1986, 7, 18, 0, 0, 0, DateTimeKind.Utc),
                Title = "Aliens",
                Year = 1986
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 98,
                Rating = "G",
                ReleaseDate = new DateTime(2008, 6, 27, 0, 0, 0, DateTimeKind.Utc),
                Title = "WALL�E",
                Year = 2008
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 137,
                Rating = "R",
                ReleaseDate = new DateTime(2006, 3, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Lives of Others",
                Year = 2006
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 136,
                Rating = "R",
                ReleaseDate = new DateTime(1972, 2, 2, 0, 0, 0, DateTimeKind.Utc),
                Title = "A Clockwork Orange",
                Year = 1971
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 155,
                Rating = "R",
                ReleaseDate = new DateTime(2000, 5, 5, 0, 0, 0, DateTimeKind.Utc),
                Title = "Gladiator",
                Year = 2000
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 189,
                Rating = "R",
                ReleaseDate = new DateTime(1999, 12, 10, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Green Mile",
                Year = 1999
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 112,
                Rating = "R",
                ReleaseDate = new DateTime(2011, 11, 2, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Intouchables",
                Year = 2011
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 227,
                Rating = "Approved",
                ReleaseDate = new DateTime(1963, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                Title = "Lawrence of Arabia",
                Year = 1962
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 129,
                Rating = "Approved",
                ReleaseDate = new DateTime(1963, 3, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "To Kill a Mockingbird",
                Year = 1962
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 130,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2006, 10, 20, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Prestige",
                Year = 2006
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 125,
                Rating = "TV-PG",
                ReleaseDate = new DateTime(1941, 3, 7, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Great Dictator",
                Year = 1940
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 99,
                Rating = "TV-MA",
                ReleaseDate = new DateTime(1992, 10, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "Reservoir Dogs",
                Year = 1992
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 149,
                Rating = "R",
                ReleaseDate = new DateTime(1982, 2, 10, 0, 0, 0, DateTimeKind.Utc),
                Title = "Das Boot",
                Year = 1981
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 102,
                Rating = "NC-17",
                ReleaseDate = new DateTime(2000, 10, 27, 0, 0, 0, DateTimeKind.Utc),
                Title = "Requiem for a Dream",
                Year = 2000
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 93,
                Rating = "Not Rated",
                ReleaseDate = new DateTime(1949, 8, 31, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Third Man",
                Year = 1949
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 126,
                Rating = "TV-PG",
                ReleaseDate = new DateTime(1948, 1, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Treasure of the Sierra Madre",
                Year = 1948
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 108,
                Rating = "R",
                ReleaseDate = new DateTime(2004, 3, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Eternal Sunshine of the Spotless Mind",
                Year = 2004
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 155,
                Rating = "PG",
                ReleaseDate = new DateTime(1990, 2, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "Cinema Paradiso",
                Year = 1988
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 139,
                Rating = "R",
                ReleaseDate = new DateTime(1984, 5, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "Once Upon a Time in America",
                Year = 1984
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 130,
                Rating = "TV-14",
                ReleaseDate = new DateTime(1974, 6, 20, 0, 0, 0, DateTimeKind.Utc),
                Title = "Chinatown",
                Year = 1974
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 138,
                Rating = "R",
                ReleaseDate = new DateTime(1997, 9, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "L.A. Confidential",
                Year = 1997
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 89,
                Rating = "G",
                ReleaseDate = new DateTime(1994, 6, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Lion King",
                Year = 1994
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 134,
                Rating = "PG",
                ReleaseDate = new DateTime(1983, 5, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "Star Wars: Episode VI - Return of the Jedi",
                Year = 1983
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 116,
                Rating = "R",
                ReleaseDate = new DateTime(1987, 6, 26, 0, 0, 0, DateTimeKind.Utc),
                Title = "Full Metal Jacket",
                Year = 1987
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 91,
                Rating = "PG",
                ReleaseDate = new DateTime(1975, 5, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "Monty Python and the Holy Grail",
                Year = 1975
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 177,
                Rating = "R",
                ReleaseDate = new DateTime(1995, 5, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "Braveheart",
                Year = 1995
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 103,
                Rating = "Approved",
                ReleaseDate = new DateTime(1952, 4, 11, 0, 0, 0, DateTimeKind.Utc),
                Title = "Singin' in the Rain",
                Year = 1952
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 120,
                Rating = "R",
                ReleaseDate = new DateTime(2003, 11, 21, 0, 0, 0, DateTimeKind.Utc),
                Title = "Oldboy",
                Year = 2003
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 120,
                Rating = "Approved",
                ReleaseDate = new DateTime(1959, 3, 29, 0, 0, 0, DateTimeKind.Utc),
                Title = "Some Like It Hot",
                Year = 1959
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 160,
                Rating = "PG",
                ReleaseDate = new DateTime(1984, 9, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Amadeus",
                Year = 1984
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 114,
                Rating = "Not Rated",
                ReleaseDate = new DateTime(1927, 3, 13, 0, 0, 0, DateTimeKind.Utc),
                Title = "Metropolis",
                Year = 1927
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 88,
                Rating = "Unrated",
                ReleaseDate = new DateTime(1951, 12, 26, 0, 0, 0, DateTimeKind.Utc),
                Title = "Rashomon",
                Year = 1950
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 93,
                Rating = "Not Rated",
                ReleaseDate = new DateTime(1949, 12, 13, 0, 0, 0, DateTimeKind.Utc),
                Title = "Bicycle Thieves",
                Year = 1948
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 141,
                Rating = "TV-PG",
                ReleaseDate = new DateTime(1968, 4, 6, 0, 0, 0, DateTimeKind.Utc),
                Title = "2001: A Space Odyssey",
                Year = 1968
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 131,
                Rating = "R",
                ReleaseDate = new DateTime(1992, 8, 7, 0, 0, 0, DateTimeKind.Utc),
                Title = "Unforgiven",
                Year = 1992
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 138,
                Rating = "Approved",
                ReleaseDate = new DateTime(1951, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "All About Eve",
                Year = 1950
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 125,
                Rating = "TV-PG",
                ReleaseDate = new DateTime(1960, 9, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Apartment",
                Year = 1960
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 127,
                Rating = "E",
                ReleaseDate = new DateTime(1989, 5, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "Indiana Jones and the Last Crusade",
                Year = 1989
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 129,
                Rating = "TV-14",
                ReleaseDate = new DateTime(1974, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Sting",
                Year = 1973
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 129,
                Rating = "R",
                ReleaseDate = new DateTime(1980, 12, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Raging Bull",
                Year = 1980
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 161,
                Rating = "Approved",
                ReleaseDate = new DateTime(1957, 12, 14, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Bridge on the River Kwai",
                Year = 1957
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 131,
                Rating = "R",
                ReleaseDate = new DateTime(1988, 7, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "Die Hard",
                Year = 1988
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 116,
                Rating = "Not Rated",
                ReleaseDate = new DateTime(1958, 2, 6, 0, 0, 0, DateTimeKind.Utc),
                Title = "Witness for the Prosecution",
                Year = 1957
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 140,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2005, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "Batman Begins",
                Year = 2005
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 123,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2011, 3, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "A Separation",
                Year = 2011
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 89,
                Rating = "Unrated",
                ReleaseDate = new DateTime(1988, 4, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "Grave of the Fireflies",
                Year = 1988
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 118,
                Rating = "R",
                ReleaseDate = new DateTime(2007, 1, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Pan's Labyrinth",
                Year = 2006
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 156,
                Rating = "R",
                ReleaseDate = new DateTime(2004, 9, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "Downfall",
                Year = 2004
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 129,
                Rating = "Approved",
                ReleaseDate = new DateTime(1939, 10, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Mr. Smith Goes to Washington",
                Year = 1939
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 75,
                Rating = "TV-MA",
                ReleaseDate = new DateTime(1961, 9, 13, 0, 0, 0, DateTimeKind.Utc),
                Title = "Yojimbo",
                Year = 1961
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 172,
                Rating = "Approved",
                ReleaseDate = new DateTime(1963, 7, 4, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Great Escape",
                Year = 1963
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 132,
                Rating = "M",
                ReleaseDate = new DateTime(1967, 5, 10, 0, 0, 0, DateTimeKind.Utc),
                Title = "For a Few Dollars More",
                Year = 1965
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 102,
                Rating = "R",
                ReleaseDate = new DateTime(2001, 1, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Snatch.",
                Year = 2000
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 153,
                Rating = "R",
                ReleaseDate = new DateTime(2009, 8, 21, 0, 0, 0, DateTimeKind.Utc),
                Title = "Inglourious Basterds",
                Year = 2009
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 108,
                Rating = "TV-PG",
                ReleaseDate = new DateTime(1954, 6, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "On the Waterfront",
                Year = 1954
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 124,
                Rating = "PG",
                ReleaseDate = new DateTime(1980, 10, 10, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Elephant Man",
                Year = 1980
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 96,
                Rating = "TV-PG",
                ReleaseDate = new DateTime(1958, 10, 13, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Seventh Seal",
                Year = 1957
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 81,
                Rating = "TV-G",
                ReleaseDate = new DateTime(1995, 11, 22, 0, 0, 0, DateTimeKind.Utc),
                Title = "Toy Story",
                Year = 1995
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 100,
                Rating = "TV-PG",
                ReleaseDate = new DateTime(1941, 10, 18, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Maltese Falcon",
                Year = 1941
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 170,
                Rating = "R",
                ReleaseDate = new DateTime(1995, 12, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "Heat",
                Year = 1995
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 75,
                Rating = "TV-G",
                ReleaseDate = new DateTime(1927, 2, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "The General",
                Year = 1926
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 116,
                Rating = "R",
                ReleaseDate = new DateTime(2009, 1, 9, 0, 0, 0, DateTimeKind.Utc),
                Title = "Gran Torino",
                Year = 2008
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 130,
                Rating = "Approved",
                ReleaseDate = new DateTime(1940, 4, 12, 0, 0, 0, DateTimeKind.Utc),
                Title = "Rebecca",
                Year = 1940
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 117,
                Rating = "R",
                ReleaseDate = new DateTime(1982, 6, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "Blade Runner",
                Year = 1982
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 143,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2012, 5, 4, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Avengers",
                Year = 2012
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 91,
                Rating = "Unrated",
                ReleaseDate = new DateTime(1959, 6, 22, 0, 0, 0, DateTimeKind.Utc),
                Title = "Wild Strawberries",
                Year = 1957
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 98,
                Rating = "R",
                ReleaseDate = new DateTime(1996, 4, 5, 0, 0, 0, DateTimeKind.Utc),
                Title = "Fargo",
                Year = 1996
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 68,
                Rating = "Not Rated",
                ReleaseDate = new DateTime(1921, 2, 6, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Kid",
                Year = 1921
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 170,
                Rating = "R",
                ReleaseDate = new DateTime(1983, 12, 9, 0, 0, 0, DateTimeKind.Utc),
                Title = "Scarface",
                Year = 1983
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 108,
                Rating = "PG-13",
                ReleaseDate = new DateTime(1958, 6, 8, 0, 0, 0, DateTimeKind.Utc),
                Title = "Touch of Evil",
                Year = 1958
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 117,
                Rating = "R",
                ReleaseDate = new DateTime(1998, 3, 6, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Big Lebowski",
                Year = 1998
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 162,
                Rating = "R",
                ReleaseDate = new DateTime(1985, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                Title = "Ran",
                Year = 1985
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 182,
                Rating = "R",
                ReleaseDate = new DateTime(1979, 2, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Deer Hunter",
                Year = 1978
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 126,
                Rating = "Approved",
                ReleaseDate = new DateTime(1967, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                Title = "Cool Hand Luke",
                Year = 1967
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 147,
                Rating = "Unrated",
                ReleaseDate = new DateTime(2005, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                Title = "Sin City",
                Year = 2005
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 72,
                Rating = "Not Rated",
                ReleaseDate = new DateTime(1925, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Gold Rush",
                Year = 1925
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 101,
                Rating = "TV-PG",
                ReleaseDate = new DateTime(1951, 6, 30, 0, 0, 0, DateTimeKind.Utc),
                Title = "Strangers on a Train",
                Year = 1951
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 105,
                Rating = "Approved",
                ReleaseDate = new DateTime(1934, 2, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "It Happened One Night",
                Year = 1934
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 122,
                Rating = "R",
                ReleaseDate = new DateTime(2007, 11, 21, 0, 0, 0, DateTimeKind.Utc),
                Title = "No Country for Old Men",
                Year = 2007
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 130,
                Rating = "TV-14",
                ReleaseDate = new DateTime(1975, 6, 20, 0, 0, 0, DateTimeKind.Utc),
                Title = "Jaws",
                Year = 1975
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 107,
                Rating = "R",
                ReleaseDate = new DateTime(1999, 3, 5, 0, 0, 0, DateTimeKind.Utc),
                Title = "Lock, Stock and Two Smoking Barrels",
                Year = 1998
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 107,
                Rating = "PG-13",
                ReleaseDate = new DateTime(1999, 8, 6, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Sixth Sense",
                Year = 1999
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 121,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2005, 2, 4, 0, 0, 0, DateTimeKind.Utc),
                Title = "Hotel Rwanda",
                Year = 2004
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 85,
                Rating = "Passed",
                ReleaseDate = new DateTime(1952, 7, 30, 0, 0, 0, DateTimeKind.Utc),
                Title = "High Noon",
                Year = 1952
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 120,
                Rating = "R",
                ReleaseDate = new DateTime(1986, 12, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "Platoon",
                Year = 1986
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 109,
                Rating = "R",
                ReleaseDate = new DateTime(1982, 6, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Thing",
                Year = 1982
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 110,
                Rating = "M",
                ReleaseDate = new DateTime(1969, 10, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "Butch Cassidy and the Sundance Kid",
                Year = 1969
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 101,
                Rating = "Approved",
                ReleaseDate = new DateTime(1939, 8, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Wizard of Oz",
                Year = 1939
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 178,
                Rating = "R",
                ReleaseDate = new DateTime(1995, 11, 22, 0, 0, 0, DateTimeKind.Utc),
                Title = "Casino",
                Year = 1995
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 94,
                Rating = "TV-MA",
                ReleaseDate = new DateTime(1996, 7, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Trainspotting",
                Year = 1996
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 111,
                Rating = "TV-14",
                ReleaseDate = new DateTime(2003, 10, 10, 0, 0, 0, DateTimeKind.Utc),
                Title = "Kill Bill: Vol. 1",
                Year = 2003
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 140,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2011, 9, 9, 0, 0, 0, DateTimeKind.Utc),
                Title = "Warrior",
                Year = 2011
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 93,
                Rating = "PG",
                ReleaseDate = new DateTime(1977, 4, 20, 0, 0, 0, DateTimeKind.Utc),
                Title = "Annie Hall",
                Year = 1977
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 101,
                Rating = "Unrated",
                ReleaseDate = new DateTime(1946, 9, 6, 0, 0, 0, DateTimeKind.Utc),
                Title = "Notorious",
                Year = 1946
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 129,
                Rating = "R",
                ReleaseDate = new DateTime(2009, 8, 13, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Secret in Their Eyes",
                Year = 2009
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 238,
                Rating = "TV-PG",
                ReleaseDate = new DateTime(1940, 1, 17, 0, 0, 0, DateTimeKind.Utc),
                Title = "Gone with the Wind",
                Year = 1939
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 126,
                Rating = "R",
                ReleaseDate = new DateTime(1998, 1, 9, 0, 0, 0, DateTimeKind.Utc),
                Title = "Good Will Hunting",
                Year = 1997
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 118,
                Rating = "R",
                ReleaseDate = new DateTime(2010, 12, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "The King's Speech",
                Year = 2010
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 129,
                Rating = "Approved",
                ReleaseDate = new DateTime(1940, 3, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Grapes of Wrath",
                Year = 1940
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 148,
                Rating = "R",
                ReleaseDate = new DateTime(2007, 9, 21, 0, 0, 0, DateTimeKind.Utc),
                Title = "Into the Wild",
                Year = 2007
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 94,
                Rating = "R",
                ReleaseDate = new DateTime(1979, 8, 17, 0, 0, 0, DateTimeKind.Utc),
                Title = "Life of Brian",
                Year = 1979
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 100,
                Rating = "G",
                ReleaseDate = new DateTime(2003, 5, 30, 0, 0, 0, DateTimeKind.Utc),
                Title = "Finding Nemo",
                Year = 2003
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 132,
                Rating = "R",
                ReleaseDate = new DateTime(2006, 3, 17, 0, 0, 0, DateTimeKind.Utc),
                Title = "V for Vendetta",
                Year = 2005
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 98,
                Rating = "PG",
                ReleaseDate = new DateTime(2010, 3, 26, 0, 0, 0, DateTimeKind.Utc),
                Title = "How to Train Your Dragon",
                Year = 2010
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 86,
                Rating = "G",
                ReleaseDate = new DateTime(1988, 4, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "My Neighbor Totoro",
                Year = 1988
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 114,
                Rating = "TV-PG",
                ReleaseDate = new DateTime(1946, 8, 31, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Big Sleep",
                Year = 1946
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 105,
                Rating = "PG",
                ReleaseDate = new DateTime(1954, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                Title = "Dial M for Murder",
                Year = 1954
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 212,
                Rating = "Approved",
                ReleaseDate = new DateTime(1960, 3, 30, 0, 0, 0, DateTimeKind.Utc),
                Title = "Ben-Hur",
                Year = 1959
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 107,
                Rating = "R",
                ReleaseDate = new DateTime(1984, 10, 26, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Terminator",
                Year = 1984
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 121,
                Rating = "R",
                ReleaseDate = new DateTime(1976, 11, 27, 0, 0, 0, DateTimeKind.Utc),
                Title = "Network",
                Year = 1976
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 132,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2005, 1, 28, 0, 0, 0, DateTimeKind.Utc),
                Title = "Million Dollar Baby",
                Year = 2004
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 108,
                Rating = "R",
                ReleaseDate = new DateTime(2010, 12, 17, 0, 0, 0, DateTimeKind.Utc),
                Title = "Black Swan",
                Year = 2010
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 93,
                Rating = "Unrated",
                ReleaseDate = new DateTime(1955, 11, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Night of the Hunter",
                Year = 1955
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 158,
                Rating = "R",
                ReleaseDate = new DateTime(2008, 1, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "There Will Be Blood",
                Year = 2007
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 89,
                Rating = "R",
                ReleaseDate = new DateTime(1986, 8, 8, 0, 0, 0, DateTimeKind.Utc),
                Title = "Stand by Me",
                Year = 1986
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 113,
                Rating = "R",
                ReleaseDate = new DateTime(2002, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                Title = "Donnie Darko",
                Year = 2001
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 101,
                Rating = "PG",
                ReleaseDate = new DateTime(1993, 2, 12, 0, 0, 0, DateTimeKind.Utc),
                Title = "Groundhog Day",
                Year = 1993
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 125,
                Rating = "R",
                ReleaseDate = new DateTime(1975, 9, 21, 0, 0, 0, DateTimeKind.Utc),
                Title = "Dog Day Afternoon",
                Year = 1975
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 129,
                Rating = "R",
                ReleaseDate = new DateTime(1996, 1, 5, 0, 0, 0, DateTimeKind.Utc),
                Title = "Twelve Monkeys",
                Year = 1995
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 154,
                Rating = "R",
                ReleaseDate = new DateTime(2000, 6, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "Amores Perros",
                Year = 2000
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 115,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2007, 8, 3, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Bourne Ultimatum",
                Year = 2007
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 92,
                Rating = "Not Rated",
                ReleaseDate = new DateTime(2009, 4, 9, 0, 0, 0, DateTimeKind.Utc),
                Title = "Mary and Max",
                Year = 2009
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 99,
                Rating = "Not Rated",
                ReleaseDate = new DateTime(1959, 11, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "The 400 Blows",
                Year = 1959
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 83,
                Rating = "Not Rated",
                ReleaseDate = new DateTime(1967, 3, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "Persona",
                Year = 1966
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 106,
                Rating = "Approved",
                ReleaseDate = new DateTime(1967, 12, 22, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Graduate",
                Year = 1967
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 191,
                Rating = "PG",
                ReleaseDate = new DateTime(1983, 2, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "Gandhi",
                Year = 1982
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 85,
                Rating = "Not Rated",
                ReleaseDate = new DateTime(1956, 6, 6, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Killing",
                Year = 1956
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 119,
                Rating = "TV-PG",
                ReleaseDate = new DateTime(2005, 6, 17, 0, 0, 0, DateTimeKind.Utc),
                Title = "Howl's Moving Castle",
                Year = 2004
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 100,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2012, 1, 20, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Artist",
                Year = 2011
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 98,
                Rating = "PG",
                ReleaseDate = new DateTime(1987, 9, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Princess Bride",
                Year = 1987
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 120,
                Rating = "R",
                ReleaseDate = new DateTime(2012, 10, 12, 0, 0, 0, DateTimeKind.Utc),
                Title = "Argo",
                Year = 2012
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 120,
                Rating = "R",
                ReleaseDate = new DateTime(2009, 1, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "Slumdog Millionaire",
                Year = 2008
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 131,
                Rating = "Approved",
                ReleaseDate = new DateTime(1966, 6, 22, 0, 0, 0, DateTimeKind.Utc),
                Title = "Who's Afraid of Virginia Woolf?",
                Year = 1966
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 108,
                Rating = "TV-PG",
                ReleaseDate = new DateTime(1956, 7, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "La Strada",
                Year = 1954
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 126,
                Rating = "Approved",
                ReleaseDate = new DateTime(1962, 10, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Manchurian Candidate",
                Year = 1962
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 134,
                Rating = "Approved",
                ReleaseDate = new DateTime(1961, 9, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Hustler",
                Year = 1961
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 135,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2002, 1, 4, 0, 0, 0, DateTimeKind.Utc),
                Title = "A Beautiful Mind",
                Year = 2001
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 145,
                Rating = "R",
                ReleaseDate = new DateTime(1969, 6, 18, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Wild Bunch",
                Year = 1969
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 119,
                Rating = "PG",
                ReleaseDate = new DateTime(1976, 12, 3, 0, 0, 0, DateTimeKind.Utc),
                Title = "Rocky",
                Year = 1976
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 160,
                Rating = "TV-PG",
                ReleaseDate = new DateTime(1959, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                Title = "Anatomy of a Murder",
                Year = 1959
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 120,
                Rating = "TV-PG",
                ReleaseDate = new DateTime(1953, 8, 10, 0, 0, 0, DateTimeKind.Utc),
                Title = "Stalag 17",
                Year = 1953
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 122,
                Rating = "R",
                ReleaseDate = new DateTime(1974, 3, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Exorcist",
                Year = 1973
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 138,
                Rating = "PG",
                ReleaseDate = new DateTime(1972, 12, 10, 0, 0, 0, DateTimeKind.Utc),
                Title = "Sleuth",
                Year = 1972
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 80,
                Rating = "Approved",
                ReleaseDate = new DateTime(1948, 8, 28, 0, 0, 0, DateTimeKind.Utc),
                Title = "Rope",
                Year = 1948
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 184,
                Rating = "PG",
                ReleaseDate = new DateTime(1975, 12, 18, 0, 0, 0, DateTimeKind.Utc),
                Title = "Barry Lyndon",
                Year = 1975
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 123,
                Rating = "Approved",
                ReleaseDate = new DateTime(1962, 4, 22, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Man Who Shot Liberty Valance",
                Year = 1962
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 112,
                Rating = "R",
                ReleaseDate = new DateTime(2009, 8, 14, 0, 0, 0, DateTimeKind.Utc),
                Title = "District 9",
                Year = 2009
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 163,
                Rating = "Not Rated",
                ReleaseDate = new DateTime(1980, 4, 17, 0, 0, 0, DateTimeKind.Utc),
                Title = "Stalker",
                Year = 1979
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 101,
                Rating = "R",
                ReleaseDate = new DateTime(2002, 12, 12, 0, 0, 0, DateTimeKind.Utc),
                Title = "Infernal Affairs",
                Year = 2002
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 118,
                Rating = "TV-G",
                ReleaseDate = new DateTime(1953, 9, 2, 0, 0, 0, DateTimeKind.Utc),
                Title = "Roman Holiday",
                Year = 1953
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 103,
                Rating = "PG",
                ReleaseDate = new DateTime(1998, 6, 5, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Truman Show",
                Year = 1998
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 111,
                Rating = "G",
                ReleaseDate = new DateTime(2007, 6, 29, 0, 0, 0, DateTimeKind.Utc),
                Title = "Ratatouille",
                Year = 2007
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 143,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2003, 7, 9, 0, 0, 0, DateTimeKind.Utc),
                Title = "Pirates of the Caribbean: The Curse of the Black Pearl",
                Year = 2003
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 106,
                Rating = "R",
                ReleaseDate = new DateTime(2008, 12, 12, 0, 0, 0, DateTimeKind.Utc),
                Title = "Ip Man",
                Year = 2008
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 112,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2007, 5, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Diving Bell and the Butterfly",
                Year = 2007
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 130,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2011, 7, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "Harry Potter and the Deathly Hallows: Part 2",
                Year = 2011
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 99,
                Rating = "M",
                ReleaseDate = new DateTime(1967, 1, 18, 0, 0, 0, DateTimeKind.Utc),
                Title = "A Fistful of Dollars",
                Year = 1964
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 125,
                Rating = "GP",
                ReleaseDate = new DateTime(1951, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                Title = "A Streetcar Named Desire",
                Year = 1951
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 92,
                Rating = "G",
                ReleaseDate = new DateTime(2001, 11, 2, 0, 0, 0, DateTimeKind.Utc),
                Title = "Monsters, Inc.",
                Year = 2001
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 133,
                Rating = "R",
                ReleaseDate = new DateTime(1994, 2, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "In the Name of the Father",
                Year = 1993
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 127,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2009, 5, 8, 0, 0, 0, DateTimeKind.Utc),
                Title = "Star Trek",
                Year = 2009
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 84,
                Rating = "G",
                ReleaseDate = new DateTime(1991, 11, 22, 0, 0, 0, DateTimeKind.Utc),
                Title = "Beauty and the Beast",
                Year = 1991
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 136,
                Rating = "R",
                ReleaseDate = new DateTime(1968, 6, 12, 0, 0, 0, DateTimeKind.Utc),
                Title = "Rosemary's Baby",
                Year = 1968
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 104,
                Rating = "Not Rated",
                ReleaseDate = new DateTime(1950, 10, 13, 0, 0, 0, DateTimeKind.Utc),
                Title = "Harvey",
                Year = 1950
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 109,
                Rating = "R",
                ReleaseDate = new DateTime(2009, 1, 30, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Wrestler",
                Year = 2008
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 133,
                Rating = "TV-PG",
                ReleaseDate = new DateTime(1930, 8, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "All Quiet on the Western Front",
                Year = 1930
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 98,
                Rating = "Not Rated",
                ReleaseDate = new DateTime(1996, 2, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "La Haine",
                Year = 1995
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 133,
                Rating = "R",
                ReleaseDate = new DateTime(1988, 12, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "Rain Man",
                Year = 1988
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 66,
                Rating = "TV-G",
                ReleaseDate = new DateTime(1925, 12, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "Battleship Potemkin",
                Year = 1925
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 138,
                Rating = "R",
                ReleaseDate = new DateTime(2010, 2, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Shutter Island",
                Year = 2010
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 81,
                Rating = "TV-PG",
                ReleaseDate = new DateTime(1929, 6, 3, 0, 0, 0, DateTimeKind.Utc),
                Title = "Nosferatu",
                Year = 1922
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 103,
                Rating = "R",
                ReleaseDate = new DateTime(2003, 9, 19, 0, 0, 0, DateTimeKind.Utc),
                Title = "Spring, Summer, Fall, Winter... and Spring",
                Year = 2003
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 96,
                Rating = "R",
                ReleaseDate = new DateTime(1979, 4, 25, 0, 0, 0, DateTimeKind.Utc),
                Title = "Manhattan",
                Year = 1979
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 138,
                Rating = "R",
                ReleaseDate = new DateTime(2003, 10, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "Mystic River",
                Year = 2003
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 102,
                Rating = "TV-G",
                ReleaseDate = new DateTime(1938, 2, 18, 0, 0, 0, DateTimeKind.Utc),
                Title = "Bringing Up Baby",
                Year = 1938
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 108,
                Rating = "TV-PG",
                ReleaseDate = new DateTime(1943, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                Title = "Shadow of a Doubt",
                Year = 1943
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 125,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2004, 1, 9, 0, 0, 0, DateTimeKind.Utc),
                Title = "Big Fish",
                Year = 2003
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 124,
                Rating = "TV-PG",
                ReleaseDate = new DateTime(1986, 8, 2, 0, 0, 0, DateTimeKind.Utc),
                Title = "Castle in the Sky",
                Year = 1986
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 151,
                Rating = "PG",
                ReleaseDate = new DateTime(1973, 12, 16, 0, 0, 0, DateTimeKind.Utc),
                Title = "Papillon",
                Year = 1973
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 76,
                Rating = "PG",
                ReleaseDate = new DateTime(1993, 10, 29, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Nightmare Before Christmas",
                Year = 1993
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 119,
                Rating = "R",
                ReleaseDate = new DateTime(1987, 6, 3, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Untouchables",
                Year = 1987
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 127,
                Rating = "PG-13",
                ReleaseDate = new DateTime(1993, 6, 11, 0, 0, 0, DateTimeKind.Utc),
                Title = "Jurassic Park",
                Year = 1993
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 115,
                Rating = "R",
                ReleaseDate = new DateTime(2008, 10, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "Let the Right One In",
                Year = 2008
            },
            new Movie
            {
                BestPictureWinner = true,
                Duration = 109,
                Rating = "TV-14",
                ReleaseDate = new DateTime(1967, 10, 14, 0, 0, 0, DateTimeKind.Utc),
                Title = "In the Heat of the Night",
                Year = 1967
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 170,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2009, 12, 24, 0, 0, 0, DateTimeKind.Utc),
                Title = "3 Idiots",
                Year = 2009
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 118,
                Rating = "Approved",
                ReleaseDate = new DateTime(1944, 9, 23, 0, 0, 0, DateTimeKind.Utc),
                Title = "Arsenic and Old Lace",
                Year = 1944
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 119,
                Rating = "Approved",
                ReleaseDate = new DateTime(1956, 3, 13, 0, 0, 0, DateTimeKind.Utc),
                Title = "The Searchers",
                Year = 1956
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 98,
                Rating = "PG",
                ReleaseDate = new DateTime(2000, 9, 29, 0, 0, 0, DateTimeKind.Utc),
                Title = "In the Mood for Love",
                Year = 2000
            },
            new Movie
            {
                BestPictureWinner = false,
                Duration = 141,
                Rating = "Approved",
                ReleaseDate = new DateTime(1959, 4, 4, 0, 0, 0, DateTimeKind.Utc),
                Title = "Rio Bravo",
                Year = 1959
            },
        };
    }
}
