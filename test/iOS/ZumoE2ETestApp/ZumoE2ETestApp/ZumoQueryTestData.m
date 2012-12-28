//
//  ZumoQueryTestData.m
//  ZumoE2ETestApp
//
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import "ZumoQueryTestData.h"
#import "ZumoTestGlobals.h"

@implementation ZumoQueryTestData

+ (NSArray *)getMovies {

    static NSArray *allItems = nil;
    if (!allItems) {
        allItems = @[
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(142),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1994 month:10 day:14],
    @"Title": @"The Shawshank Redemption",
    @"Year": @(1994)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(175),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1972 month:3 day:24],
    @"Title": @"The Godfather",
    @"Year": @(1972)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(200),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1974 month:12 day:20],
    @"Title": @"The Godfather: Part II",
    @"Year": @(1974)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(168),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1994 month:10 day:14],
    @"Title": @"Pulp Fiction",
    @"Year": @(1994)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(161),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1967 month:12 day:29],
    @"Title": @"The Good, the Bad and the Ugly",
    @"Year": @(1966)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(96),
    @"Rating": @"TV-PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1957 month:4 day:1],
    @"Title": @"12 Angry Men",
    @"Year": @(1957)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(152),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2008 month:7 day:18],
    @"Title": @"The Dark Knight",
    @"Year": @(2008)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(195),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1993 month:12 day:15],
    @"Title": @"Schindler's List",
    @"Year": @(1993)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(201),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2003 month:12 day:17],
    @"Title": @"The Lord of the Rings: The Return of the King",
    @"Year": @(2003)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(139),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1999 month:10 day:15],
    @"Title": @"Fight Club",
    @"Year": @(1999)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(127),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1980 month:5 day:21],
    @"Title": @"Star Wars: Episode V - The Empire Strikes Back",
    @"Year": @(1980)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(133),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1975 month:11 day:21],
    @"Title": @"One Flew Over the Cuckoo's Nest",
    @"Year": @(1975)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(178),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2001 month:12 day:19],
    @"Title": @"The Lord of the Rings: The Fellowship of the Ring",
    @"Year": @(2001)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(148),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2010 month:7 day:16],
    @"Title": @"Inception",
    @"Year": @(2010)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(146),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1990 month:9 day:19],
    @"Title": @"Goodfellas",
    @"Year": @(1990)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(121),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1977 month:5 day:25],
    @"Title": @"Star Wars",
    @"Year": @(1977)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(141),
    @"Rating": @"Unrated",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1956 month:11 day:19],
    @"Title": @"Seven Samurai",
    @"Year": @(1954)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(136),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1999 month:3 day:31],
    @"Title": @"The Matrix",
    @"Year": @(1999)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(142),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1994 month:7 day:6],
    @"Title": @"Forrest Gump",
    @"Year": @(1994)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(130),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2002 month:1 day:1],
    @"Title": @"City of God",
    @"Year": @(2002)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(179),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2002 month:12 day:18],
    @"Title": @"The Lord of the Rings: The Two Towers",
    @"Year": @(2002)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(175),
    @"Rating": @"M",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1968 month:12 day:21],
    @"Title": @"Once Upon a Time in the West",
    @"Year": @(1968)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(127),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1995 month:9 day:22],
    @"Title": @"Se7en",
    @"Year": @(1995)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(118),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1991 month:2 day:14],
    @"Title": @"The Silence of the Lambs",
    @"Year": @(1991)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(102),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1943 month:1 day:23],
    @"Title": @"Casablanca",
    @"Year": @(1942)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(106),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1995 month:8 day:16],
    @"Title": @"The Usual Suspects",
    @"Year": @(1995)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(115),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1981 month:6 day:12],
    @"Title": @"Raiders of the Lost Ark",
    @"Year": @(1981)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(112),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1955 month:1 day:14],
    @"Title": @"Rear Window",
    @"Year": @(1954)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(109),
    @"Rating": @"TV-14",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1960 month:9 day:8],
    @"Title": @"Psycho",
    @"Year": @(1960)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(130),
    @"Rating": @"Unrated",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1947 month:1 day:7],
    @"Title": @"It's a Wonderful Life",
    @"Year": @(1946)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(110),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1994 month:11 day:18],
    @"Title": @"L?on: The Professional",
    @"Year": @(1994)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(110),
    @"Rating": @"Passed",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1950 month:8 day:25],
    @"Title": @"Sunset Blvd.",
    @"Year": @(1950)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(113),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2000 month:10 day:11],
    @"Title": @"Memento",
    @"Year": @(2000)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(165),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2012 month:7 day:20],
    @"Title": @"The Dark Knight Rises",
    @"Year": @(2012)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(119),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1999 month:2 day:12],
    @"Title": @"American History X",
    @"Year": @(1998)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(153),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1979 month:8 day:15],
    @"Title": @"Apocalypse Now",
    @"Year": @(1979)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(152),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1991 month:7 day:3],
    @"Title": @"Terminator 2: Judgment Day",
    @"Year": @(1991)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(95),
    @"Rating": @"TV-PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1964 month:1 day:29],
    @"Title": @"Dr. Strangelove or: How I Learned to Stop Worrying and Love the Bomb",
    @"Year": @(1964)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(169),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1998 month:7 day:24],
    @"Title": @"Saving Private Ryan",
    @"Year": @(1998)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(117),
    @"Rating": @"TV-14",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1979 month:5 day:25],
    @"Title": @"Alien",
    @"Year": @(1979)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(136),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1959 month:9 day:26],
    @"Title": @"North by Northwest",
    @"Year": @(1959)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(87),
    @"Rating": @"G",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1931 month:3 day:7],
    @"Title": @"City Lights",
    @"Year": @(1931)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(125),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2001 month:7 day:20],
    @"Title": @"Spirited Away",
    @"Year": @(2001)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(119),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1941 month:9 day:5],
    @"Title": @"Citizen Kane",
    @"Year": @(1941)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(87),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1936 month:2 day:25],
    @"Title": @"Modern Times",
    @"Year": @(1936)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(142),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1980 month:5 day:23],
    @"Title": @"The Shining",
    @"Year": @(1980)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(129),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1958 month:7 day:21],
    @"Title": @"Vertigo",
    @"Year": @(1958)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(116),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1985 month:7 day:3],
    @"Title": @"Back to the Future",
    @"Year": @(1985)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(122),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1999 month:10 day:1],
    @"Title": @"American Beauty",
    @"Year": @(1999)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(150),
    @"Rating": @"TV-MA",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2003 month:3 day:28],
    @"Title": @"The Pianist",
    @"Year": @(2002)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(151),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2006 month:10 day:6],
    @"Title": @"The Departed",
    @"Year": @(2006)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(113),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1976 month:2 day:8],
    @"Title": @"Taxi Driver",
    @"Year": @(1976)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(103),
    @"Rating": @"G",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2010 month:6 day:18],
    @"Title": @"Toy Story 3",
    @"Year": @(2010)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(88),
    @"Rating": @"TV-14",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1957 month:10 day:25],
    @"Title": @"Paths of Glory",
    @"Year": @(1957)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(118),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1999 month:2 day:12],
    @"Title": @"Life Is Beautiful",
    @"Year": @(1997)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(107),
    @"Rating": @"Passed",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1944 month:4 day:24],
    @"Title": @"Double Indemnity",
    @"Year": @(1944)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(154),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1986 month:7 day:18],
    @"Title": @"Aliens",
    @"Year": @(1986)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(98),
    @"Rating": @"G",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2008 month:6 day:27],
    @"Title": @"WALL?E",
    @"Year": @(2008)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(137),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2006 month:3 day:23],
    @"Title": @"The Lives of Others",
    @"Year": @(2006)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(136),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1972 month:2 day:2],
    @"Title": @"A Clockwork Orange",
    @"Year": @(1971)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(155),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2000 month:5 day:5],
    @"Title": @"Gladiator",
    @"Year": @(2000)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(189),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1999 month:12 day:10],
    @"Title": @"The Green Mile",
    @"Year": @(1999)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(112),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2011 month:11 day:2],
    @"Title": @"The Intouchables",
    @"Year": @(2011)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(227),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1963 month:1 day:30],
    @"Title": @"Lawrence of Arabia",
    @"Year": @(1962)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(129),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1963 month:3 day:16],
    @"Title": @"To Kill a Mockingbird",
    @"Year": @(1962)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(130),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2006 month:10 day:20],
    @"Title": @"The Prestige",
    @"Year": @(2006)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(125),
    @"Rating": @"TV-PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1941 month:3 day:7],
    @"Title": @"The Great Dictator",
    @"Year": @(1940)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(99),
    @"Rating": @"TV-MA",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1992 month:10 day:23],
    @"Title": @"Reservoir Dogs",
    @"Year": @(1992)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(149),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1982 month:2 day:10],
    @"Title": @"Das Boot",
    @"Year": @(1981)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(102),
    @"Rating": @"NC-17",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2000 month:10 day:27],
    @"Title": @"Requiem for a Dream",
    @"Year": @(2000)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(93),
    @"Rating": @"Not Rated",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1949 month:8 day:31],
    @"Title": @"The Third Man",
    @"Year": @(1949)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(126),
    @"Rating": @"TV-PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1948 month:1 day:24],
    @"Title": @"The Treasure of the Sierra Madre",
    @"Year": @(1948)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(108),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2004 month:3 day:19],
    @"Title": @"Eternal Sunshine of the Spotless Mind",
    @"Year": @(2004)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(155),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1990 month:2 day:23],
    @"Title": @"Cinema Paradiso",
    @"Year": @(1988)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(139),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1984 month:5 day:23],
    @"Title": @"Once Upon a Time in America",
    @"Year": @(1984)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(130),
    @"Rating": @"TV-14",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1974 month:6 day:20],
    @"Title": @"Chinatown",
    @"Year": @(1974)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(138),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1997 month:9 day:19],
    @"Title": @"L.A. Confidential",
    @"Year": @(1997)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(89),
    @"Rating": @"G",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1994 month:6 day:24],
    @"Title": @"The Lion King",
    @"Year": @(1994)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(134),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1983 month:5 day:25],
    @"Title": @"Star Wars: Episode VI - Return of the Jedi",
    @"Year": @(1983)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(116),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1987 month:6 day:26],
    @"Title": @"Full Metal Jacket",
    @"Year": @(1987)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(91),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1975 month:5 day:25],
    @"Title": @"Monty Python and the Holy Grail",
    @"Year": @(1975)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(177),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1995 month:5 day:24],
    @"Title": @"Braveheart",
    @"Year": @(1995)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(103),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1952 month:4 day:11],
    @"Title": @"Singin' in the Rain",
    @"Year": @(1952)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(120),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2003 month:11 day:21],
    @"Title": @"Oldboy",
    @"Year": @(2003)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(120),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1959 month:3 day:29],
    @"Title": @"Some Like It Hot",
    @"Year": @(1959)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(160),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1984 month:9 day:19],
    @"Title": @"Amadeus",
    @"Year": @(1984)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(114),
    @"Rating": @"Not Rated",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1927 month:3 day:13],
    @"Title": @"Metropolis",
    @"Year": @(1927)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(88),
    @"Rating": @"Unrated",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1951 month:12 day:26],
    @"Title": @"Rashomon",
    @"Year": @(1950)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(93),
    @"Rating": @"Not Rated",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1949 month:12 day:13],
    @"Title": @"Bicycle Thieves",
    @"Year": @(1948)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(141),
    @"Rating": @"TV-PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1968 month:4 day:6],
    @"Title": @"2001: A Space Odyssey",
    @"Year": @(1968)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(131),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1992 month:8 day:7],
    @"Title": @"Unforgiven",
    @"Year": @(1992)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(138),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1951 month:1 day:15],
    @"Title": @"All About Eve",
    @"Year": @(1950)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(125),
    @"Rating": @"TV-PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1960 month:9 day:16],
    @"Title": @"The Apartment",
    @"Year": @(1960)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(127),
    @"Rating": @"E",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1989 month:5 day:24],
    @"Title": @"Indiana Jones and the Last Crusade",
    @"Year": @(1989)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(129),
    @"Rating": @"TV-14",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1974 month:1 day:10],
    @"Title": @"The Sting",
    @"Year": @(1973)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(129),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1980 month:12 day:19],
    @"Title": @"Raging Bull",
    @"Year": @(1980)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(161),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1957 month:12 day:14],
    @"Title": @"The Bridge on the River Kwai",
    @"Year": @(1957)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(131),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1988 month:7 day:15],
    @"Title": @"Die Hard",
    @"Year": @(1988)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(116),
    @"Rating": @"Not Rated",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1958 month:2 day:6],
    @"Title": @"Witness for the Prosecution",
    @"Year": @(1957)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(140),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2005 month:6 day:15],
    @"Title": @"Batman Begins",
    @"Year": @(2005)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(123),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2011 month:3 day:16],
    @"Title": @"A Separation",
    @"Year": @(2011)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(89),
    @"Rating": @"Unrated",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1988 month:4 day:16],
    @"Title": @"Grave of the Fireflies",
    @"Year": @(1988)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(118),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2007 month:1 day:19],
    @"Title": @"Pan's Labyrinth",
    @"Year": @(2006)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(156),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2004 month:9 day:16],
    @"Title": @"Downfall",
    @"Year": @(2004)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(129),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1939 month:10 day:19],
    @"Title": @"Mr. Smith Goes to Washington",
    @"Year": @(1939)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(75),
    @"Rating": @"TV-MA",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1961 month:9 day:13],
    @"Title": @"Yojimbo",
    @"Year": @(1961)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(172),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1963 month:7 day:4],
    @"Title": @"The Great Escape",
    @"Year": @(1963)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(132),
    @"Rating": @"M",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1967 month:5 day:10],
    @"Title": @"For a Few Dollars More",
    @"Year": @(1965)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(102),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2001 month:1 day:19],
    @"Title": @"Snatch.",
    @"Year": @(2000)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(153),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2009 month:8 day:21],
    @"Title": @"Inglourious Basterds",
    @"Year": @(2009)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(108),
    @"Rating": @"TV-PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1954 month:6 day:24],
    @"Title": @"On the Waterfront",
    @"Year": @(1954)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(124),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1980 month:10 day:10],
    @"Title": @"The Elephant Man",
    @"Year": @(1980)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(96),
    @"Rating": @"TV-PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1958 month:10 day:13],
    @"Title": @"The Seventh Seal",
    @"Year": @(1957)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(81),
    @"Rating": @"TV-G",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1995 month:11 day:22],
    @"Title": @"Toy Story",
    @"Year": @(1995)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(100),
    @"Rating": @"TV-PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1941 month:10 day:18],
    @"Title": @"The Maltese Falcon",
    @"Year": @(1941)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(170),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1995 month:12 day:15],
    @"Title": @"Heat",
    @"Year": @(1995)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(75),
    @"Rating": @"TV-G",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1927 month:2 day:24],
    @"Title": @"The General",
    @"Year": @(1926)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(116),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2009 month:1 day:9],
    @"Title": @"Gran Torino",
    @"Year": @(2008)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(130),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1940 month:4 day:12],
    @"Title": @"Rebecca",
    @"Year": @(1940)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(117),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1982 month:6 day:25],
    @"Title": @"Blade Runner",
    @"Year": @(1982)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(143),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2012 month:5 day:4],
    @"Title": @"The Avengers",
    @"Year": @(2012)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(91),
    @"Rating": @"Unrated",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1959 month:6 day:22],
    @"Title": @"Wild Strawberries",
    @"Year": @(1957)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(98),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1996 month:4 day:5],
    @"Title": @"Fargo",
    @"Year": @(1996)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(68),
    @"Rating": @"Not Rated",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1921 month:2 day:6],
    @"Title": @"The Kid",
    @"Year": @(1921)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(170),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1983 month:12 day:9],
    @"Title": @"Scarface",
    @"Year": @(1983)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(108),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1958 month:6 day:8],
    @"Title": @"Touch of Evil",
    @"Year": @(1958)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(117),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1998 month:3 day:6],
    @"Title": @"The Big Lebowski",
    @"Year": @(1998)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(162),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1985 month:6 day:1],
    @"Title": @"Ran",
    @"Year": @(1985)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(182),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1979 month:2 day:23],
    @"Title": @"The Deer Hunter",
    @"Year": @(1978)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(126),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1967 month:11 day:1],
    @"Title": @"Cool Hand Luke",
    @"Year": @(1967)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(147),
    @"Rating": @"Unrated",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2005 month:4 day:1],
    @"Title": @"Sin City",
    @"Year": @(2005)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(72),
    @"Rating": @"Not Rated",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1925 month:1 day:1],
    @"Title": @"The Gold Rush",
    @"Year": @(1925)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(101),
    @"Rating": @"TV-PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1951 month:6 day:30],
    @"Title": @"Strangers on a Train",
    @"Year": @(1951)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(105),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1934 month:2 day:23],
    @"Title": @"It Happened One Night",
    @"Year": @(1934)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(122),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2007 month:11 day:21],
    @"Title": @"No Country for Old Men",
    @"Year": @(2007)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(130),
    @"Rating": @"TV-14",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1975 month:6 day:20],
    @"Title": @"Jaws",
    @"Year": @(1975)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(107),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1999 month:3 day:5],
    @"Title": @"Lock, Stock and Two Smoking Barrels",
    @"Year": @(1998)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(107),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1999 month:8 day:6],
    @"Title": @"The Sixth Sense",
    @"Year": @(1999)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(121),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2005 month:2 day:4],
    @"Title": @"Hotel Rwanda",
    @"Year": @(2004)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(85),
    @"Rating": @"Passed",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1952 month:7 day:30],
    @"Title": @"High Noon",
    @"Year": @(1952)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(120),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1986 month:12 day:24],
    @"Title": @"Platoon",
    @"Year": @(1986)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(109),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1982 month:6 day:25],
    @"Title": @"The Thing",
    @"Year": @(1982)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(110),
    @"Rating": @"M",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1969 month:10 day:24],
    @"Title": @"Butch Cassidy and the Sundance Kid",
    @"Year": @(1969)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(101),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1939 month:8 day:25],
    @"Title": @"The Wizard of Oz",
    @"Year": @(1939)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(178),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1995 month:11 day:22],
    @"Title": @"Casino",
    @"Year": @(1995)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(94),
    @"Rating": @"TV-MA",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1996 month:7 day:19],
    @"Title": @"Trainspotting",
    @"Year": @(1996)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(111),
    @"Rating": @"TV-14",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2003 month:10 day:10],
    @"Title": @"Kill Bill: Vol. 1",
    @"Year": @(2003)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(140),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2011 month:9 day:9],
    @"Title": @"Warrior",
    @"Year": @(2011)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(93),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1977 month:4 day:20],
    @"Title": @"Annie Hall",
    @"Year": @(1977)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(101),
    @"Rating": @"Unrated",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1946 month:9 day:6],
    @"Title": @"Notorious",
    @"Year": @(1946)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(129),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2009 month:8 day:13],
    @"Title": @"The Secret in Their Eyes",
    @"Year": @(2009)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(238),
    @"Rating": @"TV-PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1940 month:1 day:17],
    @"Title": @"Gone with the Wind",
    @"Year": @(1939)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(126),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1998 month:1 day:9],
    @"Title": @"Good Will Hunting",
    @"Year": @(1997)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(118),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2010 month:12 day:24],
    @"Title": @"The King's Speech",
    @"Year": @(2010)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(129),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1940 month:3 day:15],
    @"Title": @"The Grapes of Wrath",
    @"Year": @(1940)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(148),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2007 month:9 day:21],
    @"Title": @"Into the Wild",
    @"Year": @(2007)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(94),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1979 month:8 day:17],
    @"Title": @"Life of Brian",
    @"Year": @(1979)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(100),
    @"Rating": @"G",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2003 month:5 day:30],
    @"Title": @"Finding Nemo",
    @"Year": @(2003)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(132),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2006 month:3 day:17],
    @"Title": @"V for Vendetta",
    @"Year": @(2005)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(98),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2010 month:3 day:26],
    @"Title": @"How to Train Your Dragon",
    @"Year": @(2010)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(86),
    @"Rating": @"G",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1988 month:4 day:16],
    @"Title": @"My Neighbor Totoro",
    @"Year": @(1988)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(114),
    @"Rating": @"TV-PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1946 month:8 day:31],
    @"Title": @"The Big Sleep",
    @"Year": @(1946)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(105),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1954 month:5 day:29],
    @"Title": @"Dial M for Murder",
    @"Year": @(1954)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(212),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1960 month:3 day:30],
    @"Title": @"Ben-Hur",
    @"Year": @(1959)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(107),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1984 month:10 day:26],
    @"Title": @"The Terminator",
    @"Year": @(1984)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(121),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1976 month:11 day:27],
    @"Title": @"Network",
    @"Year": @(1976)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(132),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2005 month:1 day:28],
    @"Title": @"Million Dollar Baby",
    @"Year": @(2004)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(108),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2010 month:12 day:17],
    @"Title": @"Black Swan",
    @"Year": @(2010)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(93),
    @"Rating": @"Unrated",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1955 month:11 day:24],
    @"Title": @"The Night of the Hunter",
    @"Year": @(1955)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(158),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2008 month:1 day:25],
    @"Title": @"There Will Be Blood",
    @"Year": @(2007)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(89),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1986 month:8 day:8],
    @"Title": @"Stand by Me",
    @"Year": @(1986)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(113),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2002 month:1 day:30],
    @"Title": @"Donnie Darko",
    @"Year": @(2001)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(101),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1993 month:2 day:12],
    @"Title": @"Groundhog Day",
    @"Year": @(1993)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(125),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1975 month:9 day:21],
    @"Title": @"Dog Day Afternoon",
    @"Year": @(1975)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(129),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1996 month:1 day:5],
    @"Title": @"Twelve Monkeys",
    @"Year": @(1995)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(154),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2000 month:6 day:16],
    @"Title": @"Amores Perros",
    @"Year": @(2000)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(115),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2007 month:8 day:3],
    @"Title": @"The Bourne Ultimatum",
    @"Year": @(2007)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(92),
    @"Rating": @"Not Rated",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2009 month:4 day:9],
    @"Title": @"Mary and Max",
    @"Year": @(2009)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(99),
    @"Rating": @"Not Rated",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1959 month:11 day:16],
    @"Title": @"The 400 Blows",
    @"Year": @(1959)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(83),
    @"Rating": @"Not Rated",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1967 month:3 day:16],
    @"Title": @"Persona",
    @"Year": @(1966)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(106),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1967 month:12 day:22],
    @"Title": @"The Graduate",
    @"Year": @(1967)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(191),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1983 month:2 day:25],
    @"Title": @"Gandhi",
    @"Year": @(1982)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(85),
    @"Rating": @"Not Rated",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1956 month:6 day:6],
    @"Title": @"The Killing",
    @"Year": @(1956)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(119),
    @"Rating": @"TV-PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2005 month:6 day:17],
    @"Title": @"Howl's Moving Castle",
    @"Year": @(2004)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(100),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2012 month:1 day:20],
    @"Title": @"The Artist",
    @"Year": @(2011)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(98),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1987 month:9 day:25],
    @"Title": @"The Princess Bride",
    @"Year": @(1987)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(120),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2012 month:10 day:12],
    @"Title": @"Argo",
    @"Year": @(2012)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(120),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2009 month:1 day:23],
    @"Title": @"Slumdog Millionaire",
    @"Year": @(2008)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(131),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1966 month:6 day:22],
    @"Title": @"Who's Afraid of Virginia Woolf?",
    @"Year": @(1966)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(108),
    @"Rating": @"TV-PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1956 month:7 day:16],
    @"Title": @"La Strada",
    @"Year": @(1954)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(126),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1962 month:10 day:24],
    @"Title": @"The Manchurian Candidate",
    @"Year": @(1962)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(134),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1961 month:9 day:25],
    @"Title": @"The Hustler",
    @"Year": @(1961)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(135),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2002 month:1 day:4],
    @"Title": @"A Beautiful Mind",
    @"Year": @(2001)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(145),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1969 month:6 day:18],
    @"Title": @"The Wild Bunch",
    @"Year": @(1969)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(119),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1976 month:12 day:3],
    @"Title": @"Rocky",
    @"Year": @(1976)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(160),
    @"Rating": @"TV-PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1959 month:9 day:1],
    @"Title": @"Anatomy of a Murder",
    @"Year": @(1959)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(120),
    @"Rating": @"TV-PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1953 month:8 day:10],
    @"Title": @"Stalag 17",
    @"Year": @(1953)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(122),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1974 month:3 day:16],
    @"Title": @"The Exorcist",
    @"Year": @(1973)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(138),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1972 month:12 day:10],
    @"Title": @"Sleuth",
    @"Year": @(1972)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(80),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1948 month:8 day:28],
    @"Title": @"Rope",
    @"Year": @(1948)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(184),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1975 month:12 day:18],
    @"Title": @"Barry Lyndon",
    @"Year": @(1975)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(123),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1962 month:4 day:22],
    @"Title": @"The Man Who Shot Liberty Valance",
    @"Year": @(1962)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(112),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2009 month:8 day:14],
    @"Title": @"District 9",
    @"Year": @(2009)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(163),
    @"Rating": @"Not Rated",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1980 month:4 day:17],
    @"Title": @"Stalker",
    @"Year": @(1979)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(101),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2002 month:12 day:12],
    @"Title": @"Infernal Affairs",
    @"Year": @(2002)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(118),
    @"Rating": @"TV-G",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1953 month:9 day:2],
    @"Title": @"Roman Holiday",
    @"Year": @(1953)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(103),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1998 month:6 day:5],
    @"Title": @"The Truman Show",
    @"Year": @(1998)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(111),
    @"Rating": @"G",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2007 month:6 day:29],
    @"Title": @"Ratatouille",
    @"Year": @(2007)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(143),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2003 month:7 day:9],
    @"Title": @"Pirates of the Caribbean: The Curse of the Black Pearl",
    @"Year": @(2003)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(106),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2008 month:12 day:12],
    @"Title": @"Ip Man",
    @"Year": @(2008)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(112),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2007 month:5 day:23],
    @"Title": @"The Diving Bell and the Butterfly",
    @"Year": @(2007)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(130),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2011 month:7 day:15],
    @"Title": @"Harry Potter and the Deathly Hallows: Part 2",
    @"Year": @(2011)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(99),
    @"Rating": @"M",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1967 month:1 day:18],
    @"Title": @"A Fistful of Dollars",
    @"Year": @(1964)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(125),
    @"Rating": @"GP",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1951 month:12 day:1],
    @"Title": @"A Streetcar Named Desire",
    @"Year": @(1951)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(92),
    @"Rating": @"G",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2001 month:11 day:2],
    @"Title": @"Monsters, Inc.",
    @"Year": @(2001)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(133),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1994 month:2 day:25],
    @"Title": @"In the Name of the Father",
    @"Year": @(1993)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(127),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2009 month:5 day:8],
    @"Title": @"Star Trek",
    @"Year": @(2009)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(84),
    @"Rating": @"G",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1991 month:11 day:22],
    @"Title": @"Beauty and the Beast",
    @"Year": @(1991)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(136),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1968 month:6 day:12],
    @"Title": @"Rosemary's Baby",
    @"Year": @(1968)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(104),
    @"Rating": @"Not Rated",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1950 month:10 day:13],
    @"Title": @"Harvey",
    @"Year": @(1950)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(109),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2009 month:1 day:30],
    @"Title": @"The Wrestler",
    @"Year": @(2008)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(133),
    @"Rating": @"TV-PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1930 month:8 day:24],
    @"Title": @"All Quiet on the Western Front",
    @"Year": @(1930)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(98),
    @"Rating": @"Not Rated",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1996 month:2 day:23],
    @"Title": @"La Haine",
    @"Year": @(1995)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(133),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1988 month:12 day:16],
    @"Title": @"Rain Man",
    @"Year": @(1988)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(66),
    @"Rating": @"TV-G",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1925 month:12 day:24],
    @"Title": @"Battleship Potemkin",
    @"Year": @(1925)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(138),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2010 month:2 day:19],
    @"Title": @"Shutter Island",
    @"Year": @(2010)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(81),
    @"Rating": @"TV-PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1929 month:6 day:3],
    @"Title": @"Nosferatu",
    @"Year": @(1922)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(103),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2003 month:9 day:19],
    @"Title": @"Spring, Summer, Fall, Winter... and Spring",
    @"Year": @(2003)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(96),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1979 month:4 day:25],
    @"Title": @"Manhattan",
    @"Year": @(1979)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(138),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2003 month:10 day:15],
    @"Title": @"Mystic River",
    @"Year": @(2003)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(102),
    @"Rating": @"TV-G",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1938 month:2 day:18],
    @"Title": @"Bringing Up Baby",
    @"Year": @(1938)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(108),
    @"Rating": @"TV-PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1943 month:1 day:15],
    @"Title": @"Shadow of a Doubt",
    @"Year": @(1943)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(125),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2004 month:1 day:9],
    @"Title": @"Big Fish",
    @"Year": @(2003)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(124),
    @"Rating": @"TV-PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1986 month:8 day:2],
    @"Title": @"Castle in the Sky",
    @"Year": @(1986)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(151),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1973 month:12 day:16],
    @"Title": @"Papillon",
    @"Year": @(1973)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(76),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1993 month:10 day:29],
    @"Title": @"The Nightmare Before Christmas",
    @"Year": @(1993)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(119),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1987 month:6 day:3],
    @"Title": @"The Untouchables",
    @"Year": @(1987)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(127),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1993 month:6 day:11],
    @"Title": @"Jurassic Park",
    @"Year": @(1993)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(115),
    @"Rating": @"R",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2008 month:10 day:24],
    @"Title": @"Let the Right One In",
    @"Year": @(2008)
    },
    @{
    @"BestPictureWinner": @YES,
    @"Duration": @(109),
    @"Rating": @"TV-14",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1967 month:10 day:14],
    @"Title": @"In the Heat of the Night",
    @"Year": @(1967)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(170),
    @"Rating": @"PG-13",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2009 month:12 day:24],
    @"Title": @"3 Idiots",
    @"Year": @(2009)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(118),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1944 month:9 day:23],
    @"Title": @"Arsenic and Old Lace",
    @"Year": @(1944)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(119),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1956 month:3 day:13],
    @"Title": @"The Searchers",
    @"Year": @(1956)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(98),
    @"Rating": @"PG",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:2000 month:9 day:29],
    @"Title": @"In the Mood for Love",
    @"Year": @(2000)
    },
    @{
    @"BestPictureWinner": @NO,
    @"Duration": @(141),
    @"Rating": @"Approved",
    @"ReleaseDate": [ZumoTestGlobals createDateWithYear:1959 month:4 day:4],
    @"Title": @"Rio Bravo",
    @"Year": @(1959)
    }
    ];
    }

    return allItems;
    
}
@end
