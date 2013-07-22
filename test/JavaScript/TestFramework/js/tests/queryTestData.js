// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="../testFramework.js" />

function createQueryTestData() {
    var movies = [];

    movies.push({
        Title: "The Shawshank Redemption",
        Year: 1994,
        Duration: 142,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1994, 9, 14)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Godfather",
        Year: 1972,
        Duration: 175,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1972, 2, 24)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "The Godfather: Part II",
        Year: 1974,
        Duration: 200,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1974, 11, 20)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "Pulp Fiction",
        Year: 1994,
        Duration: 168,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1994, 9, 14)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Good, the Bad and the Ugly",
        Year: 1966,
        Duration: 161,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1967, 11, 28)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "12 Angry Men",
        Year: 1957,
        Duration: 96,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1957, 2, 31)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Dark Knight",
        Year: 2008,
        Duration: 152,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2008, 6, 18)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Schindler's List",
        Year: 1993,
        Duration: 195,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1993, 11, 15)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "The Lord of the Rings: The Return of the King",
        Year: 2003,
        Duration: 201,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2003, 11, 17)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "Fight Club",
        Year: 1999,
        Duration: 139,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1999, 9, 15)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Star Wars: Episode V - The Empire Strikes Back",
        Year: 1980,
        Duration: 127,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1980, 4, 21)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "One Flew Over the Cuckoo's Nest",
        Year: 1975,
        Duration: 133,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1975, 10, 21)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "The Lord of the Rings: The Fellowship of the Ring",
        Year: 2001,
        Duration: 178,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2001, 11, 19)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Inception",
        Year: 2010,
        Duration: 148,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2010, 6, 16)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Goodfellas",
        Year: 1990,
        Duration: 146,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1990, 8, 19)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Star Wars",
        Year: 1977,
        Duration: 121,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1977, 4, 25)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Seven Samurai",
        Year: 1954,
        Duration: 141,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1956, 10, 18)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Matrix",
        Year: 1999,
        Duration: 136,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1999, 2, 31)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Forrest Gump",
        Year: 1994,
        Duration: 142,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(1994, 6, 06)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "City of God",
        Year: 2002,
        Duration: 130,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2002, 0, 01)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Lord of the Rings: The Two Towers",
        Year: 2002,
        Duration: 179,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2002, 11, 18)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Once Upon a Time in the West",
        Year: 1968,
        Duration: 175,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(1968, 11, 20)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Se7en",
        Year: 1995,
        Duration: 127,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1995, 8, 22)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Silence of the Lambs",
        Year: 1991,
        Duration: 118,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1991, 1, 14)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "Casablanca",
        Year: 1942,
        Duration: 102,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1943, 0, 22)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "The Usual Suspects",
        Year: 1995,
        Duration: 106,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1995, 7, 16)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Raiders of the Lost Ark",
        Year: 1981,
        Duration: 115,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1981, 5, 12)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Rear Window",
        Year: 1954,
        Duration: 112,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1955, 0, 13)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Psycho",
        Year: 1960,
        Duration: 109,
        MPAARating: "TV-14",
        ReleaseDate: new Date(Date.UTC(1960, 8, 07)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "It's a Wonderful Life",
        Year: 1946,
        Duration: 130,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1947, 0, 06)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Léon: The Professional",
        Year: 1994,
        Duration: 110,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1994, 10, 18)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Sunset Blvd.",
        Year: 1950,
        Duration: 110,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1950, 7, 24)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Memento",
        Year: 2000,
        Duration: 113,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2000, 9, 11)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Dark Knight Rises",
        Year: 2012,
        Duration: 165,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2012, 6, 20)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "American History X",
        Year: 1998,
        Duration: 119,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1999, 1, 12)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Apocalypse Now",
        Year: 1979,
        Duration: 153,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1979, 7, 15)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Terminator 2: Judgment Day",
        Year: 1991,
        Duration: 152,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1991, 6, 03)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Dr. Strangelove or: How I Learned to Stop Worrying and Love the Bomb",
        Year: 1964,
        Duration: 95,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1964, 0, 28)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Saving Private Ryan",
        Year: 1998,
        Duration: 169,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1998, 6, 24)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Alien",
        Year: 1979,
        Duration: 117,
        MPAARating: "TV-14",
        ReleaseDate: new Date(Date.UTC(1979, 4, 25)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "North by Northwest",
        Year: 1959,
        Duration: 136,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1959, 8, 25)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "City Lights",
        Year: 1931,
        Duration: 87,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1931, 2, 06)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Spirited Away",
        Year: 2001,
        Duration: 125,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(2001, 6, 20)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Citizen Kane",
        Year: 1941,
        Duration: 119,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1941, 8, 04)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Modern Times",
        Year: 1936,
        Duration: 87,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1936, 1, 24)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Shining",
        Year: 1980,
        Duration: 142,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1980, 4, 23)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Vertigo",
        Year: 1958,
        Duration: 129,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1958, 6, 20)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Back to the Future",
        Year: 1985,
        Duration: 116,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1985, 6, 03)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "American Beauty",
        Year: 1999,
        Duration: 122,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1999, 9, 01)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "M",
        Year: 1931,
        Duration: 117,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1931, 7, 30)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Pianist",
        Year: 2002,
        Duration: 150,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2003, 2, 28)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Departed",
        Year: 2006,
        Duration: 151,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2006, 9, 06)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "Taxi Driver",
        Year: 1976,
        Duration: 113,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1976, 1, 08)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Toy Story 3",
        Year: 2010,
        Duration: 103,
        MPAARating: "G",
        ReleaseDate: new Date(Date.UTC(2010, 5, 18)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Paths of Glory",
        Year: 1957,
        Duration: 88,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1957, 9, 24)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Life Is Beautiful",
        Year: 1997,
        Duration: 118,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(1999, 1, 12)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Double Indemnity",
        Year: 1944,
        Duration: 107,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1944, 3, 23)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Aliens",
        Year: 1986,
        Duration: 154,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1986, 6, 18)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "WALL-E",
        Year: 2008,
        Duration: 98,
        MPAARating: "G",
        ReleaseDate: new Date(Date.UTC(2008, 5, 27)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Lives of Others",
        Year: 2006,
        Duration: 137,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2006, 2, 23)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "A Clockwork Orange",
        Year: 1971,
        Duration: 136,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1972, 1, 02)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Amélie",
        Year: 2001,
        Duration: 122,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2001, 3, 24)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Gladiator",
        Year: 2000,
        Duration: 155,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2000, 4, 05)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "The Green Mile",
        Year: 1999,
        Duration: 189,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1999, 11, 10)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Intouchables",
        Year: 2011,
        Duration: 112,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2011, 10, 02)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Lawrence of Arabia",
        Year: 1962,
        Duration: 227,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1963, 0, 29)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "To Kill a Mockingbird",
        Year: 1962,
        Duration: 129,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1963, 2, 15)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Prestige",
        Year: 2006,
        Duration: 130,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2006, 9, 20)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Great Dictator",
        Year: 1940,
        Duration: 125,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1941, 2, 06)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Reservoir Dogs",
        Year: 1992,
        Duration: 99,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1992, 9, 23)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Das Boot",
        Year: 1981,
        Duration: 149,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1982, 1, 10)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Requiem for a Dream",
        Year: 2000,
        Duration: 102,
        MPAARating: "NC-17",
        ReleaseDate: new Date(Date.UTC(2000, 9, 27)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Third Man",
        Year: 1949,
        Duration: 93,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1949, 7, 30)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Treasure of the Sierra Madre",
        Year: 1948,
        Duration: 126,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1948, 0, 23)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Eternal Sunshine of the Spotless Mind",
        Year: 2004,
        Duration: 108,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2004, 2, 19)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Cinema Paradiso",
        Year: 1988,
        Duration: 155,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1990, 1, 23)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Once Upon a Time in America",
        Year: 1984,
        Duration: 139,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1984, 4, 23)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Chinatown",
        Year: 1974,
        Duration: 130,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1974, 5, 20)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "L.A. Confidential",
        Year: 1997,
        Duration: 138,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1997, 8, 19)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Lion King",
        Year: 1994,
        Duration: 89,
        MPAARating: "G",
        ReleaseDate: new Date(Date.UTC(1994, 5, 24)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Star Wars: Episode VI - Return of the Jedi",
        Year: 1983,
        Duration: 134,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1983, 4, 25)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Full Metal Jacket",
        Year: 1987,
        Duration: 116,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1987, 5, 26)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Monty Python and the Holy Grail",
        Year: 1975,
        Duration: 91,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1975, 4, 25)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Braveheart",
        Year: 1995,
        Duration: 177,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1995, 4, 24)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "Singin' in the Rain",
        Year: 1952,
        Duration: 103,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1952, 3, 10)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Oldboy",
        Year: 2003,
        Duration: 120,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2003, 10, 21)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Some Like It Hot",
        Year: 1959,
        Duration: 120,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1959, 2, 28)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Amadeus",
        Year: 1984,
        Duration: 160,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1984, 8, 19)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "Metropolis",
        Year: 1927,
        Duration: 114,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1927, 2, 12)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Rashomon",
        Year: 1950,
        Duration: 88,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1951, 11, 25)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Bicycle Thieves",
        Year: 1948,
        Duration: 93,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1949, 11, 12)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "2001: A Space Odyssey",
        Year: 1968,
        Duration: 141,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1968, 3, 05)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Unforgiven",
        Year: 1992,
        Duration: 131,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1992, 7, 07)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "All About Eve",
        Year: 1950,
        Duration: 138,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1951, 0, 14)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "The Apartment",
        Year: 1960,
        Duration: 125,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1960, 8, 15)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "Indiana Jones and the Last Crusade",
        Year: 1989,
        Duration: 127,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1989, 4, 24)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Sting",
        Year: 1973,
        Duration: 129,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1974, 0, 10)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "Raging Bull",
        Year: 1980,
        Duration: 129,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1980, 11, 19)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Bridge on the River Kwai",
        Year: 1957,
        Duration: 161,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1957, 11, 13)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "Die Hard",
        Year: 1988,
        Duration: 131,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1988, 6, 15)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Witness for the Prosecution",
        Year: 1957,
        Duration: 116,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1958, 1, 05)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Batman Begins",
        Year: 2005,
        Duration: 140,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2005, 5, 15)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "A Separation",
        Year: 2011,
        Duration: 123,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2011, 2, 16)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Grave of the Fireflies",
        Year: 1988,
        Duration: 89,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1988, 3, 16)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Pan's Labyrinth",
        Year: 2006,
        Duration: 118,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2007, 0, 19)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Downfall",
        Year: 2004,
        Duration: 156,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2004, 8, 16)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Mr. Smith Goes to Washington",
        Year: 1939,
        Duration: 129,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1939, 9, 18)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Yojimbo",
        Year: 1961,
        Duration: 75,
        MPAARating: "TV-MA",
        ReleaseDate: new Date(Date.UTC(1961, 8, 12)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Great Escape",
        Year: 1963,
        Duration: 172,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1963, 6, 03)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "For a Few Dollars More",
        Year: 1965,
        Duration: 132,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1967, 4, 09)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Snatch.",
        Year: 2000,
        Duration: 102,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2001, 0, 19)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Inglourious Basterds",
        Year: 2009,
        Duration: 153,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2009, 7, 21)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "On the Waterfront",
        Year: 1954,
        Duration: 108,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1954, 5, 23)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "The Elephant Man",
        Year: 1980,
        Duration: 124,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1980, 9, 10)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Seventh Seal",
        Year: 1957,
        Duration: 96,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1958, 9, 12)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Toy Story",
        Year: 1995,
        Duration: 81,
        MPAARating: "TV-G",
        ReleaseDate: new Date(Date.UTC(1995, 10, 22)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Maltese Falcon",
        Year: 1941,
        Duration: 100,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1941, 9, 17)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Heat",
        Year: 1995,
        Duration: 170,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1995, 11, 15)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The General",
        Year: 1926,
        Duration: 75,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1927, 1, 23)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Gran Torino",
        Year: 2008,
        Duration: 116,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2009, 0, 09)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Rebecca",
        Year: 1940,
        Duration: 130,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1940, 3, 11)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "Blade Runner",
        Year: 1982,
        Duration: 117,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1982, 5, 25)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Avengers",
        Year: 2012,
        Duration: 143,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2012, 4, 04)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Wild Strawberries",
        Year: 1957,
        Duration: 91,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1959, 5, 21)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Fargo",
        Year: 1996,
        Duration: 98,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1996, 3, 05)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Kid",
        Year: 1921,
        Duration: 68,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1921, 1, 05)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Scarface",
        Year: 1983,
        Duration: 170,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1983, 11, 09)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Touch of Evil",
        Year: 1958,
        Duration: 108,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(1958, 5, 07)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Big Lebowski",
        Year: 1998,
        Duration: 117,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1998, 2, 06)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Ran",
        Year: 1985,
        Duration: 162,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1985, 5, 01)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Deer Hunter",
        Year: 1978,
        Duration: 182,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1979, 1, 23)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "Cool Hand Luke",
        Year: 1967,
        Duration: 126,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1967, 9, 31)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Sin City",
        Year: 2005,
        Duration: 147,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2005, 3, 01)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Gold Rush",
        Year: 1925,
        Duration: 72,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1924, 11, 31)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Strangers on a Train",
        Year: 1951,
        Duration: 101,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1951, 5, 29)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "It Happened One Night",
        Year: 1934,
        Duration: 105,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1934, 1, 22)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "No Country for Old Men",
        Year: 2007,
        Duration: 122,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2007, 10, 21)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "Jaws",
        Year: 1975,
        Duration: 130,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1975, 5, 20)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Lock, Stock and Two Smoking Barrels",
        Year: 1998,
        Duration: 107,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1999, 2, 05)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Sixth Sense",
        Year: 1999,
        Duration: 107,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(1999, 7, 06)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Hotel Rwanda",
        Year: 2004,
        Duration: 121,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2005, 1, 04)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "High Noon",
        Year: 1952,
        Duration: 85,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1952, 6, 29)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Platoon",
        Year: 1986,
        Duration: 120,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1986, 11, 24)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "The Thing",
        Year: 1982,
        Duration: 109,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1982, 5, 25)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Butch Cassidy and the Sundance Kid",
        Year: 1969,
        Duration: 110,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1969, 9, 23)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Wizard of Oz",
        Year: 1939,
        Duration: 101,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1939, 7, 24)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Casino",
        Year: 1995,
        Duration: 178,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1995, 10, 22)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Trainspotting",
        Year: 1996,
        Duration: 94,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1996, 6, 19)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Kill Bill: Vol. 1",
        Year: 2003,
        Duration: 111,
        MPAARating: "TV-14",
        ReleaseDate: new Date(Date.UTC(2003, 9, 10)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Warrior",
        Year: 2011,
        Duration: 140,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2011, 8, 09)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Annie Hall",
        Year: 1977,
        Duration: 93,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1977, 3, 20)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "Notorious",
        Year: 1946,
        Duration: 101,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1946, 8, 05)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Secret in Their Eyes",
        Year: 2009,
        Duration: 129,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2009, 7, 13)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Gone with the Wind",
        Year: 1939,
        Duration: 238,
        MPAARating: "G",
        ReleaseDate: new Date(Date.UTC(1940, 0, 16)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "Good Will Hunting",
        Year: 1997,
        Duration: 126,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1998, 0, 09)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The King's Speech",
        Year: 2010,
        Duration: 118,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2010, 11, 24)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "The Grapes of Wrath",
        Year: 1940,
        Duration: 129,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1940, 2, 14)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Into the Wild",
        Year: 2007,
        Duration: 148,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2007, 8, 21)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Life of Brian",
        Year: 1979,
        Duration: 94,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1979, 7, 17)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Finding Nemo",
        Year: 2003,
        Duration: 100,
        MPAARating: "G",
        ReleaseDate: new Date(Date.UTC(2003, 4, 30)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "V for Vendetta",
        Year: 2005,
        Duration: 132,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2006, 2, 17)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "How to Train Your Dragon",
        Year: 2010,
        Duration: 98,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(2010, 2, 26)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "My Neighbor Totoro",
        Year: 1988,
        Duration: 86,
        MPAARating: "G",
        ReleaseDate: new Date(Date.UTC(1988, 3, 16)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Big Sleep",
        Year: 1946,
        Duration: 114,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1946, 7, 30)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Dial M for Murder",
        Year: 1954,
        Duration: 105,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1954, 4, 28)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Ben-Hur",
        Year: 1959,
        Duration: 212,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1960, 2, 29)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "The Terminator",
        Year: 1984,
        Duration: 107,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1984, 9, 26)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Network",
        Year: 1976,
        Duration: 121,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1976, 10, 27)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Million Dollar Baby",
        Year: 2004,
        Duration: 132,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2005, 0, 28)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "Black Swan",
        Year: 2010,
        Duration: 108,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2010, 11, 17)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Night of the Hunter",
        Year: 1955,
        Duration: 93,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1955, 10, 23)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "There Will Be Blood",
        Year: 2007,
        Duration: 158,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2008, 0, 25)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Stand by Me",
        Year: 1986,
        Duration: 89,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1986, 7, 08)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Donnie Darko",
        Year: 2001,
        Duration: 113,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2002, 0, 30)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Groundhog Day",
        Year: 1993,
        Duration: 101,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1993, 1, 12)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Dog Day Afternoon",
        Year: 1975,
        Duration: 125,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1975, 8, 21)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Twelve Monkeys",
        Year: 1995,
        Duration: 129,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1996, 0, 05)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Amores Perros",
        Year: 2000,
        Duration: 154,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2000, 5, 16)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Bourne Ultimatum",
        Year: 2007,
        Duration: 115,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2007, 7, 03)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Mary and Max",
        Year: 2009,
        Duration: 92,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(2009, 3, 09)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The 400 Blows",
        Year: 1959,
        Duration: 99,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1959, 10, 15)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Persona",
        Year: 1966,
        Duration: 83,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1967, 2, 15)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Graduate",
        Year: 1967,
        Duration: 106,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1967, 11, 21)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Gandhi",
        Year: 1982,
        Duration: 191,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1983, 1, 25)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "The Killing",
        Year: 1956,
        Duration: 85,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1956, 5, 05)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Howl's Moving Castle",
        Year: 2004,
        Duration: 119,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(2005, 5, 17)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Artist",
        Year: 2011,
        Duration: 100,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2012, 0, 20)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "The Princess Bride",
        Year: 1987,
        Duration: 98,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1987, 8, 25)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Argo",
        Year: 2012,
        Duration: 120,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2012, 9, 12)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Slumdog Millionaire",
        Year: 2008,
        Duration: 120,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2009, 0, 23)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "Who's Afraid of Virginia Woolf?",
        Year: 1966,
        Duration: 131,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1966, 5, 21)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "La Strada",
        Year: 1954,
        Duration: 108,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1956, 6, 15)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Manchurian Candidate",
        Year: 1962,
        Duration: 126,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1962, 9, 23)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Hustler",
        Year: 1961,
        Duration: 134,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1961, 8, 24)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "A Beautiful Mind",
        Year: 2001,
        Duration: 135,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2002, 0, 04)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "The Wild Bunch",
        Year: 1969,
        Duration: 145,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1969, 5, 17)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Rocky",
        Year: 1976,
        Duration: 119,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1976, 11, 03)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "Anatomy of a Murder",
        Year: 1959,
        Duration: 160,
        MPAARating: "TV-PG",
        ReleaseDate: new Date(Date.UTC(1959, 7, 31)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Stalag 17",
        Year: 1953,
        Duration: 120,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1953, 7, 09)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Exorcist",
        Year: 1973,
        Duration: 122,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1974, 2, 16)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Sleuth",
        Year: 1972,
        Duration: 138,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1972, 11, 10)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Rope",
        Year: 1948,
        Duration: 80,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1948, 7, 27)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Barry Lyndon",
        Year: 1975,
        Duration: 184,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1975, 11, 18)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Man Who Shot Liberty Valance",
        Year: 1962,
        Duration: 123,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1962, 3, 21)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "District 9",
        Year: 2009,
        Duration: 112,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2009, 7, 14)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Stalker",
        Year: 1979,
        Duration: 163,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1980, 3, 17)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Infernal Affairs",
        Year: 2002,
        Duration: 101,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2002, 11, 12)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Roman Holiday",
        Year: 1953,
        Duration: 118,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1953, 8, 01)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Truman Show",
        Year: 1998,
        Duration: 103,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1998, 5, 05)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Ratatouille",
        Year: 2007,
        Duration: 111,
        MPAARating: "G",
        ReleaseDate: new Date(Date.UTC(2007, 5, 29)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Pirates of the Caribbean: The Curse of the Black Pearl",
        Year: 2003,
        Duration: 143,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2003, 6, 09)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Ip Man",
        Year: 2008,
        Duration: 106,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2008, 11, 12)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Diving Bell and the Butterfly",
        Year: 2007,
        Duration: 112,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2007, 4, 23)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Harry Potter and the Deathly Hallows: Part 2",
        Year: 2011,
        Duration: 130,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2011, 6, 15)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "A Fistful of Dollars",
        Year: 1964,
        Duration: 99,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1967, 0, 17)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "A Streetcar Named Desire",
        Year: 1951,
        Duration: 125,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1951, 10, 30)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Monsters, Inc.",
        Year: 2001,
        Duration: 92,
        MPAARating: "G",
        ReleaseDate: new Date(Date.UTC(2001, 10, 02)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "In the Name of the Father",
        Year: 1993,
        Duration: 133,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1994, 1, 25)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Star Trek",
        Year: 2009,
        Duration: 127,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2009, 4, 08)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Beauty and the Beast",
        Year: 1991,
        Duration: 84,
        MPAARating: "G",
        ReleaseDate: new Date(Date.UTC(1991, 10, 22)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Rosemary's Baby",
        Year: 1968,
        Duration: 136,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1968, 5, 11)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Harvey",
        Year: 1950,
        Duration: 104,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1950, 9, 12)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Nauticaä of the Valley of the Wind",
        Year: 1984,
        Duration: 117,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1984, 3, 11)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Wrestler",
        Year: 2008,
        Duration: 109,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2009, 0, 30)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "All Quiet on the Western Front",
        Year: 1930,
        Duration: 133,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1930, 7, 23)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "La Haine",
        Year: 1995,
        Duration: 98,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1996, 1, 23)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Rain Man",
        Year: 1988,
        Duration: 133,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1988, 11, 16)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "Battleship Potemkin",
        Year: 1925,
        Duration: 66,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1925, 11, 23)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Shutter Island",
        Year: 2010,
        Duration: 138,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2010, 1, 19)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Nosferatu",
        Year: 1922,
        Duration: 81,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1929, 5, 02)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Spring, Summer, Fall, Winter... and Spring",
        Year: 2003,
        Duration: 103,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2003, 8, 19)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Manhattan",
        Year: 1979,
        Duration: 96,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1979, 3, 25)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Mystic River",
        Year: 2003,
        Duration: 138,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2003, 9, 15)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Bringing Up Baby",
        Year: 1938,
        Duration: 102,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1938, 1, 17)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Shadow of a Doubt",
        Year: 1943,
        Duration: 108,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1943, 0, 14)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Big Fish",
        Year: 2003,
        Duration: 125,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2004, 0, 09)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Castle in the Sky",
        Year: 1986,
        Duration: 124,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1986, 7, 02)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Papillon",
        Year: 1973,
        Duration: 151,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1973, 11, 16)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Nightmare Before Christmas",
        Year: 1993,
        Duration: 76,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(1993, 9, 29)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Untouchables",
        Year: 1987,
        Duration: 119,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(1987, 5, 03)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Jurassic Park",
        Year: 1993,
        Duration: 127,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(1993, 5, 11)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Let the Right One In",
        Year: 2008,
        Duration: 115,
        MPAARating: "R",
        ReleaseDate: new Date(Date.UTC(2008, 9, 24)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "In the Heat of the Night",
        Year: 1967,
        Duration: 109,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1967, 9, 13)),
        BestPictureWinner: true
    });
    movies.push({
        Title: "3 Idiots",
        Year: 2009,
        Duration: 170,
        MPAARating: "PG-13",
        ReleaseDate: new Date(Date.UTC(2009, 11, 24)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Arsenic and Old Lace",
        Year: 1944,
        Duration: 118,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1944, 8, 22)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "The Searchers",
        Year: 1956,
        Duration: 119,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1956, 2, 12)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "In the Mood for Love",
        Year: 2000,
        Duration: 98,
        MPAARating: "PG",
        ReleaseDate: new Date(Date.UTC(2000, 8, 29)),
        BestPictureWinner: false
    });
    movies.push({
        Title: "Rio Bravo",
        Year: 1959,
        Duration: 141,
        MPAARating: null,
        ReleaseDate: new Date(Date.UTC(1959, 3, 03)),
        BestPictureWinner: false
    });

    return movies;
}

zumo.tests.getQueryTestData = createQueryTestData;
