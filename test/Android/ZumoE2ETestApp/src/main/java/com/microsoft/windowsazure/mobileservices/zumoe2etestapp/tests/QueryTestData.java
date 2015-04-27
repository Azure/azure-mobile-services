/*
Copyright (c) Microsoft Open Technologies, Inc.
All Rights Reserved
Apache 2.0 License
 
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
 
     http://www.apache.org/licenses/LICENSE-2.0
 
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 
See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.
 */
package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests;

import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.IntIdMovie;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.StringIdMovie;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;

import static com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.Util.getUTCDate;

public class QueryTestData {
    private static List<IntIdMovie> mAllIntIdMovies;

    static {
        mAllIntIdMovies = new ArrayList<IntIdMovie>();

        mAllIntIdMovies.add(new IntIdMovie(false, 142, "R", getUTCDate(1994, 10, 14), "The Shawshank Redemption", 1994));
        mAllIntIdMovies.add(new IntIdMovie(true, 175, "R", getUTCDate(1972, 3, 24), "The Godfather", 1972));
        mAllIntIdMovies.add(new IntIdMovie(true, 200, "R", getUTCDate(1974, 12, 20), "The Godfather: Part II", 1974));
        mAllIntIdMovies.add(new IntIdMovie(false, 168, "R", getUTCDate(1994, 10, 14), "Pulp Fiction", 1994));
        mAllIntIdMovies.add(new IntIdMovie(false, 161, null, getUTCDate(1967, 12, 29), "The Good, the Bad and the Ugly", 1966));
        mAllIntIdMovies.add(new IntIdMovie(false, 96, null, getUTCDate(1957, 4, 10), "12 Angry Men", 1957));
        mAllIntIdMovies.add(new IntIdMovie(false, 152, "PG-13", getUTCDate(2008, 7, 18), "The Dark Knight", 2008));
        mAllIntIdMovies.add(new IntIdMovie(true, 195, "R", getUTCDate(1993, 12, 15), "Schindler's List", 1993));
        mAllIntIdMovies.add(new IntIdMovie(true, 201, "PG-13", getUTCDate(2003, 12, 17), "The Lord of the Rings: The Return of the King", 2003));
        mAllIntIdMovies.add(new IntIdMovie(false, 139, "R", getUTCDate(1999, 10, 15), "Fight Club", 1999));
        mAllIntIdMovies.add(new IntIdMovie(false, 127, "PG", getUTCDate(1980, 5, 21), "Star Wars: Episode V - The Empire Strikes Back", 1980));
        mAllIntIdMovies.add(new IntIdMovie(true, 133, null, getUTCDate(1975, 11, 21), "One Flew Over the Cuckoo's Nest", 1975));
        mAllIntIdMovies.add(new IntIdMovie(false, 178, "PG-13", getUTCDate(2001, 12, 19), "The Lord of the Rings: The Fellowship of the Ring", 2001));
        mAllIntIdMovies.add(new IntIdMovie(false, 148, "PG-13", getUTCDate(2010, 7, 16), "Inception", 2010));
        mAllIntIdMovies.add(new IntIdMovie(false, 146, "R", getUTCDate(1990, 9, 19), "Goodfellas", 1990));
        mAllIntIdMovies.add(new IntIdMovie(false, 121, "PG", getUTCDate(1977, 5, 25), "Star Wars", 1977));
        mAllIntIdMovies.add(new IntIdMovie(false, 141, null, getUTCDate(1956, 11, 19), "Seven Samurai", 1954));
        mAllIntIdMovies.add(new IntIdMovie(false, 136, "R", getUTCDate(1999, 3, 31), "The Matrix", 1999));
        mAllIntIdMovies.add(new IntIdMovie(true, 142, "PG-13", getUTCDate(1994, 7, 6), "Forrest Gump", 1994));
        mAllIntIdMovies.add(new IntIdMovie(false, 130, "R", getUTCDate(2002, 1, 1), "City of God", 2002));
        mAllIntIdMovies.add(new IntIdMovie(false, 179, "PG-13", getUTCDate(2002, 12, 18), "The Lord of the Rings: The Two Towers", 2002));
        mAllIntIdMovies.add(new IntIdMovie(false, 175, "PG-13", getUTCDate(1968, 12, 21), "Once Upon a Time in the West", 1968));
        mAllIntIdMovies.add(new IntIdMovie(false, 127, "R", getUTCDate(1995, 9, 22), "Se7en", 1995));
        mAllIntIdMovies.add(new IntIdMovie(true, 118, "R", getUTCDate(1991, 2, 14), "The Silence of the Lambs", 1991));
        mAllIntIdMovies.add(new IntIdMovie(true, 102, "PG", getUTCDate(1943, 1, 23), "Casablanca", 1942));
        mAllIntIdMovies.add(new IntIdMovie(false, 106, "R", getUTCDate(1995, 8, 16), "The Usual Suspects", 1995));
        mAllIntIdMovies.add(new IntIdMovie(false, 115, "PG", getUTCDate(1981, 6, 12), "Raiders of the Lost Ark", 1981));
        mAllIntIdMovies.add(new IntIdMovie(false, 112, "PG", getUTCDate(1955, 1, 13), "Rear Window", 1954));
        mAllIntIdMovies.add(new IntIdMovie(false, 109, "TV-14", getUTCDate(1960, 9, 8), "Psycho", 1960));
        mAllIntIdMovies.add(new IntIdMovie(false, 130, "PG", getUTCDate(1947, 1, 6), "It's a Wonderful Life", 1946));
        mAllIntIdMovies.add(new IntIdMovie(false, 110, "R", getUTCDate(1994, 11, 18), "Léon: The Professional", 1994));
        mAllIntIdMovies.add(new IntIdMovie(false, 110, null, getUTCDate(1950, 8, 25), "Sunset Blvd.", 1950));
        mAllIntIdMovies.add(new IntIdMovie(false, 113, "R", getUTCDate(2000, 10, 11), "Memento", 2000));
        mAllIntIdMovies.add(new IntIdMovie(false, 165, "PG-13", getUTCDate(2012, 7, 20), "The Dark Knight Rises", 2012));
        mAllIntIdMovies.add(new IntIdMovie(false, 119, "R", getUTCDate(1999, 2, 12), "American History X", 1998));
        mAllIntIdMovies.add(new IntIdMovie(false, 153, "R", getUTCDate(1979, 8, 15), "Apocalypse Now", 1979));
        mAllIntIdMovies.add(new IntIdMovie(false, 152, "R", getUTCDate(1991, 7, 3), "Terminator 2: Judgment Day", 1991));
        mAllIntIdMovies.add(new IntIdMovie(false, 95, "PG", getUTCDate(1964, 1, 29), "Dr. Strangelove or: How I Learned to Stop Worrying and Love the Bomb",
                1964));
        mAllIntIdMovies.add(new IntIdMovie(false, 169, "R", getUTCDate(1998, 7, 24), "Saving Private Ryan", 1998));
        mAllIntIdMovies.add(new IntIdMovie(false, 117, "TV-14", getUTCDate(1979, 5, 25), "Alien", 1979));
        mAllIntIdMovies.add(new IntIdMovie(false, 136, null, getUTCDate(1959, 9, 26), "North by Northwest", 1959));
        mAllIntIdMovies.add(new IntIdMovie(false, 87, null, getUTCDate(1931, 3, 7), "City Lights", 1931));
        mAllIntIdMovies.add(new IntIdMovie(false, 125, "PG", getUTCDate(2001, 7, 20), "Spirited Away", 2001));
        mAllIntIdMovies.add(new IntIdMovie(false, 119, "PG", getUTCDate(1941, 9, 5), "Citizen Kane", 1941));
        mAllIntIdMovies.add(new IntIdMovie(false, 87, null, getUTCDate(1936, 2, 25), "Modern Times", 1936));
        mAllIntIdMovies.add(new IntIdMovie(false, 142, "R", getUTCDate(1980, 5, 23), "The Shining", 1980));
        mAllIntIdMovies.add(new IntIdMovie(false, 129, null, getUTCDate(1958, 7, 21), "Vertigo", 1958));
        mAllIntIdMovies.add(new IntIdMovie(false, 116, "PG", getUTCDate(1985, 7, 3), "Back to the Future", 1985));
        mAllIntIdMovies.add(new IntIdMovie(true, 122, "R", getUTCDate(1999, 10, 1), "American Beauty", 1999));
        mAllIntIdMovies.add(new IntIdMovie(false, 117, null, getUTCDate(1931, 8, 30), "M", 1931));
        mAllIntIdMovies.add(new IntIdMovie(false, 150, "R", getUTCDate(2003, 3, 28), "The Pianist", 2002));
        mAllIntIdMovies.add(new IntIdMovie(true, 151, "R", getUTCDate(2006, 10, 6), "The Departed", 2006));
        mAllIntIdMovies.add(new IntIdMovie(false, 113, "R", getUTCDate(1976, 2, 8), "Taxi Driver", 1976));
        mAllIntIdMovies.add(new IntIdMovie(false, 103, "G", getUTCDate(2010, 6, 18), "Toy Story 3", 2010));
        mAllIntIdMovies.add(new IntIdMovie(false, 88, null, getUTCDate(1957, 10, 25), "Paths of Glory", 1957));
        mAllIntIdMovies.add(new IntIdMovie(false, 118, "PG-13", getUTCDate(1999, 2, 12), "Life Is Beautiful", 1997));
        mAllIntIdMovies.add(new IntIdMovie(false, 107, null, getUTCDate(1944, 4, 24), "Double Indemnity", 1944));
        mAllIntIdMovies.add(new IntIdMovie(false, 154, "R", getUTCDate(1986, 7, 18), "Aliens", 1986));
        mAllIntIdMovies.add(new IntIdMovie(false, 98, "G", getUTCDate(2008, 6, 27), "WALL-E", 2008));
        mAllIntIdMovies.add(new IntIdMovie(false, 137, "R", getUTCDate(2006, 3, 23), "The Lives of Others", 2006));
        mAllIntIdMovies.add(new IntIdMovie(false, 136, "R", getUTCDate(1972, 2, 2), "A Clockwork Orange", 1971));
        mAllIntIdMovies.add(new IntIdMovie(false, 122, "R", getUTCDate(2001, 4, 24), "Amélie", 2001));
        mAllIntIdMovies.add(new IntIdMovie(true, 155, "R", getUTCDate(2000, 5, 5), "Gladiator", 2000));
        mAllIntIdMovies.add(new IntIdMovie(false, 189, "R", getUTCDate(1999, 12, 10), "The Green Mile", 1999));
        mAllIntIdMovies.add(new IntIdMovie(false, 112, "R", getUTCDate(2011, 11, 2), "The Intouchables", 2011));
        mAllIntIdMovies.add(new IntIdMovie(true, 227, null, getUTCDate(1963, 1, 30), "Lawrence of Arabia", 1962));
        mAllIntIdMovies.add(new IntIdMovie(false, 129, null, getUTCDate(1963, 3, 16), "To Kill a Mockingbird", 1962));
        mAllIntIdMovies.add(new IntIdMovie(false, 130, "PG-13", getUTCDate(2006, 10, 20), "The Prestige", 2006));
        mAllIntIdMovies.add(new IntIdMovie(false, 125, null, getUTCDate(1941, 3, 7), "The Great Dictator", 1940));
        mAllIntIdMovies.add(new IntIdMovie(false, 99, "R", getUTCDate(1992, 10, 23), "Reservoir Dogs", 1992));
        mAllIntIdMovies.add(new IntIdMovie(false, 149, "R", getUTCDate(1982, 2, 10), "Das Boot", 1981));
        mAllIntIdMovies.add(new IntIdMovie(false, 102, "NC-17", getUTCDate(2000, 10, 27), "Requiem for a Dream", 2000));
        mAllIntIdMovies.add(new IntIdMovie(false, 93, null, getUTCDate(1949, 8, 31), "The Third Man", 1949));
        mAllIntIdMovies.add(new IntIdMovie(false, 126, null, getUTCDate(1948, 1, 24), "The Treasure of the Sierra Madre", 1948));
        mAllIntIdMovies.add(new IntIdMovie(false, 108, "R", getUTCDate(2004, 3, 19), "Eternal Sunshine of the Spotless Mind", 2004));
        mAllIntIdMovies.add(new IntIdMovie(false, 155, "PG", getUTCDate(1990, 2, 23), "Cinema Paradiso", 1988));
        mAllIntIdMovies.add(new IntIdMovie(false, 139, "R", getUTCDate(1984, 5, 23), "Once Upon a Time in America", 1984));
        mAllIntIdMovies.add(new IntIdMovie(false, 130, null, getUTCDate(1974, 6, 20), "Chinatown", 1974));
        mAllIntIdMovies.add(new IntIdMovie(false, 138, "R", getUTCDate(1997, 9, 19), "L.A. Confidential", 1997));
        mAllIntIdMovies.add(new IntIdMovie(false, 89, "G", getUTCDate(1994, 6, 24), "The Lion King", 1994));
        mAllIntIdMovies.add(new IntIdMovie(false, 134, "PG", getUTCDate(1983, 5, 25), "Star Wars: Episode VI - Return of the Jedi", 1983));
        mAllIntIdMovies.add(new IntIdMovie(false, 116, "R", getUTCDate(1987, 6, 26), "Full Metal Jacket", 1987));
        mAllIntIdMovies.add(new IntIdMovie(false, 91, "PG", getUTCDate(1975, 5, 25), "Monty Python and the Holy Grail", 1975));
        mAllIntIdMovies.add(new IntIdMovie(true, 177, "R", getUTCDate(1995, 5, 24), "Braveheart", 1995));
        mAllIntIdMovies.add(new IntIdMovie(false, 103, null, getUTCDate(1952, 4, 11), "Singin' in the Rain", 1952));
        mAllIntIdMovies.add(new IntIdMovie(false, 120, "R", getUTCDate(2003, 11, 21), "Oldboy", 2003));
        mAllIntIdMovies.add(new IntIdMovie(false, 120, null, getUTCDate(1959, 3, 29), "Some Like It Hot", 1959));
        mAllIntIdMovies.add(new IntIdMovie(true, 160, "PG", getUTCDate(1984, 9, 19), "Amadeus", 1984));
        mAllIntIdMovies.add(new IntIdMovie(false, 114, null, getUTCDate(1927, 3, 13), "Metropolis", 1927));
        mAllIntIdMovies.add(new IntIdMovie(false, 88, null, getUTCDate(1951, 12, 26), "Rashomon", 1950));
        mAllIntIdMovies.add(new IntIdMovie(false, 93, null, getUTCDate(1949, 12, 13), "Bicycle Thieves", 1948));
        mAllIntIdMovies.add(new IntIdMovie(false, 141, null, getUTCDate(1968, 4, 6), "2001: A Space Odyssey", 1968));
        mAllIntIdMovies.add(new IntIdMovie(true, 131, "R", getUTCDate(1992, 8, 7), "Unforgiven", 1992));
        mAllIntIdMovies.add(new IntIdMovie(true, 138, null, getUTCDate(1951, 1, 15), "All About Eve", 1950));
        mAllIntIdMovies.add(new IntIdMovie(true, 125, null, getUTCDate(1960, 9, 16), "The Apartment", 1960));
        mAllIntIdMovies.add(new IntIdMovie(false, 127, "PG", getUTCDate(1989, 5, 24), "Indiana Jones and the Last Crusade", 1989));
        mAllIntIdMovies.add(new IntIdMovie(true, 129, null, getUTCDate(1974, 1, 10), "The Sting", 1973));
        mAllIntIdMovies.add(new IntIdMovie(false, 129, "R", getUTCDate(1980, 12, 19), "Raging Bull", 1980));
        mAllIntIdMovies.add(new IntIdMovie(true, 161, null, getUTCDate(1957, 12, 14), "The Bridge on the River Kwai", 1957));
        mAllIntIdMovies.add(new IntIdMovie(false, 131, "R", getUTCDate(1988, 7, 15), "Die Hard", 1988));
        mAllIntIdMovies.add(new IntIdMovie(false, 116, null, getUTCDate(1958, 2, 6), "Witness for the Prosecution", 1957));
        mAllIntIdMovies.add(new IntIdMovie(false, 140, "PG-13", getUTCDate(2005, 6, 15), "Batman Begins", 2005));
        mAllIntIdMovies.add(new IntIdMovie(false, 123, "PG-13", getUTCDate(2011, 3, 16), "A Separation", 2011));
        mAllIntIdMovies.add(new IntIdMovie(false, 89, null, getUTCDate(1988, 4, 16), "Grave of the Fireflies", 1988));
        mAllIntIdMovies.add(new IntIdMovie(false, 118, "R", getUTCDate(2007, 1, 19), "Pan's Labyrinth", 2006));
        mAllIntIdMovies.add(new IntIdMovie(false, 156, "R", getUTCDate(2004, 9, 16), "Downfall", 2004));
        mAllIntIdMovies.add(new IntIdMovie(false, 129, null, getUTCDate(1939, 10, 19), "Mr. Smith Goes to Washington", 1939));
        mAllIntIdMovies.add(new IntIdMovie(false, 75, "TV-MA", getUTCDate(1961, 9, 13), "Yojimbo", 1961));
        mAllIntIdMovies.add(new IntIdMovie(false, 172, null, getUTCDate(1963, 7, 4), "The Great Escape", 1963));
        mAllIntIdMovies.add(new IntIdMovie(false, 132, "R", getUTCDate(1967, 5, 10), "For a Few Dollars More", 1965));
        mAllIntIdMovies.add(new IntIdMovie(false, 102, "R", getUTCDate(2001, 1, 19), "Snatch.", 2000));
        mAllIntIdMovies.add(new IntIdMovie(false, 153, "R", getUTCDate(2009, 8, 21), "Inglourious Basterds", 2009));
        mAllIntIdMovies.add(new IntIdMovie(true, 108, null, getUTCDate(1954, 6, 24), "On the Waterfront", 1954));
        mAllIntIdMovies.add(new IntIdMovie(false, 124, "PG", getUTCDate(1980, 10, 10), "The Elephant Man", 1980));
        mAllIntIdMovies.add(new IntIdMovie(false, 96, null, getUTCDate(1958, 10, 13), "The Seventh Seal", 1957));
        mAllIntIdMovies.add(new IntIdMovie(false, 81, "TV-G", getUTCDate(1995, 11, 22), "Toy Story", 1995));
        mAllIntIdMovies.add(new IntIdMovie(false, 100, null, getUTCDate(1941, 10, 18), "The Maltese Falcon", 1941));
        mAllIntIdMovies.add(new IntIdMovie(false, 170, "R", getUTCDate(1995, 12, 15), "Heat", 1995));
        mAllIntIdMovies.add(new IntIdMovie(false, 75, null, getUTCDate(1927, 2, 24), "The General", 1926));
        mAllIntIdMovies.add(new IntIdMovie(false, 116, "R", getUTCDate(2009, 1, 9), "Gran Torino", 2008));
        mAllIntIdMovies.add(new IntIdMovie(true, 130, null, getUTCDate(1940, 4, 12), "Rebecca", 1940));
        mAllIntIdMovies.add(new IntIdMovie(false, 117, "R", getUTCDate(1982, 6, 25), "Blade Runner", 1982));
        mAllIntIdMovies.add(new IntIdMovie(false, 143, "PG-13", getUTCDate(2012, 5, 4), "The Avengers", 2012));
        mAllIntIdMovies.add(new IntIdMovie(false, 91, null, getUTCDate(1959, 6, 22), "Wild Strawberries", 1957));
        mAllIntIdMovies.add(new IntIdMovie(false, 98, "R", getUTCDate(1996, 4, 5), "Fargo", 1996));
        mAllIntIdMovies.add(new IntIdMovie(false, 68, null, getUTCDate(1921, 2, 6), "The Kid", 1921));
        mAllIntIdMovies.add(new IntIdMovie(false, 170, "R", getUTCDate(1983, 12, 9), "Scarface", 1983));
        mAllIntIdMovies.add(new IntIdMovie(false, 108, "PG-13", getUTCDate(1958, 6, 8), "Touch of Evil", 1958));
        mAllIntIdMovies.add(new IntIdMovie(false, 117, "R", getUTCDate(1998, 3, 6), "The Big Lebowski", 1998));
        mAllIntIdMovies.add(new IntIdMovie(false, 162, "R", getUTCDate(1985, 6, 1), "Ran", 1985));
        mAllIntIdMovies.add(new IntIdMovie(true, 182, "R", getUTCDate(1979, 2, 23), "The Deer Hunter", 1978));
        mAllIntIdMovies.add(new IntIdMovie(false, 126, null, getUTCDate(1967, 11, 1), "Cool Hand Luke", 1967));
        mAllIntIdMovies.add(new IntIdMovie(false, 147, "R", getUTCDate(2005, 4, 1), "Sin City", 2005));
        mAllIntIdMovies.add(new IntIdMovie(false, 72, null, getUTCDate(1925, 6, 26), "The Gold Rush", 1925));
        mAllIntIdMovies.add(new IntIdMovie(false, 101, null, getUTCDate(1951, 6, 30), "Strangers on a Train", 1951));
        mAllIntIdMovies.add(new IntIdMovie(true, 105, null, getUTCDate(1934, 2, 23), "It Happened One Night", 1934));
        mAllIntIdMovies.add(new IntIdMovie(true, 122, "R", getUTCDate(2007, 11, 21), "No Country for Old Men", 2007));
        mAllIntIdMovies.add(new IntIdMovie(false, 130, "PG", getUTCDate(1975, 6, 20), "Jaws", 1975));
        mAllIntIdMovies.add(new IntIdMovie(false, 107, "R", getUTCDate(1999, 3, 5), "Lock, Stock and Two Smoking Barrels", 1998));
        mAllIntIdMovies.add(new IntIdMovie(false, 107, "PG-13", getUTCDate(1999, 8, 6), "The Sixth Sense", 1999));
        mAllIntIdMovies.add(new IntIdMovie(false, 121, "PG-13", getUTCDate(2005, 2, 4), "Hotel Rwanda", 2004));
        mAllIntIdMovies.add(new IntIdMovie(false, 85, null, getUTCDate(1952, 7, 30), "High Noon", 1952));
        mAllIntIdMovies.add(new IntIdMovie(true, 120, "R", getUTCDate(1986, 12, 24), "Platoon", 1986));
        mAllIntIdMovies.add(new IntIdMovie(false, 109, "R", getUTCDate(1982, 6, 25), "The Thing", 1982));
        mAllIntIdMovies.add(new IntIdMovie(false, 110, "PG", getUTCDate(1969, 10, 24), "Butch Cassidy and the Sundance Kid", 1969));
        mAllIntIdMovies.add(new IntIdMovie(false, 101, null, getUTCDate(1939, 8, 25), "The Wizard of Oz", 1939));
        mAllIntIdMovies.add(new IntIdMovie(false, 178, "R", getUTCDate(1995, 11, 22), "Casino", 1995));
        mAllIntIdMovies.add(new IntIdMovie(false, 94, "R", getUTCDate(1996, 7, 19), "Trainspotting", 1996));
        mAllIntIdMovies.add(new IntIdMovie(false, 111, "TV-14", getUTCDate(2003, 10, 10), "Kill Bill: Vol. 1", 2003));
        mAllIntIdMovies.add(new IntIdMovie(false, 140, "PG-13", getUTCDate(2011, 9, 9), "Warrior", 2011));
        mAllIntIdMovies.add(new IntIdMovie(true, 93, "PG", getUTCDate(1977, 4, 20), "Annie Hall", 1977));
        mAllIntIdMovies.add(new IntIdMovie(false, 101, null, getUTCDate(1946, 9, 6), "Notorious", 1946));
        mAllIntIdMovies.add(new IntIdMovie(false, 129, "R", getUTCDate(2009, 8, 13), "The Secret in Their Eyes", 2009));
        mAllIntIdMovies.add(new IntIdMovie(true, 238, "G", getUTCDate(1940, 1, 17), "Gone with the Wind", 1939));
        mAllIntIdMovies.add(new IntIdMovie(false, 126, "R", getUTCDate(1998, 1, 9), "Good Will Hunting", 1997));
        mAllIntIdMovies.add(new IntIdMovie(true, 118, "R", getUTCDate(2010, 12, 24), "The King's Speech", 2010));
        mAllIntIdMovies.add(new IntIdMovie(false, 129, null, getUTCDate(1940, 3, 15), "The Grapes of Wrath", 1940));
        mAllIntIdMovies.add(new IntIdMovie(false, 148, "R", getUTCDate(2007, 9, 21), "Into the Wild", 2007));
        mAllIntIdMovies.add(new IntIdMovie(false, 94, "R", getUTCDate(1979, 8, 17), "Life of Brian", 1979));
        mAllIntIdMovies.add(new IntIdMovie(false, 100, "G", getUTCDate(2003, 5, 30), "Finding Nemo", 2003));
        mAllIntIdMovies.add(new IntIdMovie(false, 132, "R", getUTCDate(2006, 3, 17), "V for Vendetta", 2005));
        mAllIntIdMovies.add(new IntIdMovie(false, 98, "PG", getUTCDate(2010, 3, 26), "How to Train Your Dragon", 2010));
        mAllIntIdMovies.add(new IntIdMovie(false, 86, "G", getUTCDate(1988, 4, 16), "My Neighbor Totoro", 1988));
        mAllIntIdMovies.add(new IntIdMovie(false, 114, null, getUTCDate(1946, 8, 31), "The Big Sleep", 1946));
        mAllIntIdMovies.add(new IntIdMovie(false, 105, "PG", getUTCDate(1954, 5, 29), "Dial M for Murder", 1954));
        mAllIntIdMovies.add(new IntIdMovie(true, 212, null, getUTCDate(1960, 3, 30), "Ben-Hur", 1959));
        mAllIntIdMovies.add(new IntIdMovie(false, 107, "R", getUTCDate(1984, 10, 26), "The Terminator", 1984));
        mAllIntIdMovies.add(new IntIdMovie(false, 121, "R", getUTCDate(1976, 11, 27), "Network", 1976));
        mAllIntIdMovies.add(new IntIdMovie(true, 132, "PG-13", getUTCDate(2005, 1, 28), "Million Dollar Baby", 2004));
        mAllIntIdMovies.add(new IntIdMovie(false, 108, "R", getUTCDate(2010, 12, 17), "Black Swan", 2010));
        mAllIntIdMovies.add(new IntIdMovie(false, 93, null, getUTCDate(1955, 11, 24), "The Night of the Hunter", 1955));
        mAllIntIdMovies.add(new IntIdMovie(false, 158, "R", getUTCDate(2008, 1, 25), "There Will Be Blood", 2007));
        mAllIntIdMovies.add(new IntIdMovie(false, 89, "R", getUTCDate(1986, 8, 8), "Stand by Me", 1986));
        mAllIntIdMovies.add(new IntIdMovie(false, 113, "R", getUTCDate(2002, 1, 30), "Donnie Darko", 2001));
        mAllIntIdMovies.add(new IntIdMovie(false, 101, "PG", getUTCDate(1993, 2, 12), "Groundhog Day", 1993));
        mAllIntIdMovies.add(new IntIdMovie(false, 125, "R", getUTCDate(1975, 9, 21), "Dog Day Afternoon", 1975));
        mAllIntIdMovies.add(new IntIdMovie(false, 129, "R", getUTCDate(1996, 1, 5), "Twelve Monkeys", 1995));
        mAllIntIdMovies.add(new IntIdMovie(false, 154, "R", getUTCDate(2000, 6, 16), "Amores Perros", 2000));
        mAllIntIdMovies.add(new IntIdMovie(false, 115, "PG-13", getUTCDate(2007, 8, 3), "The Bourne Ultimatum", 2007));
        mAllIntIdMovies.add(new IntIdMovie(false, 92, null, getUTCDate(2009, 4, 9), "Mary and Max", 2009));
        mAllIntIdMovies.add(new IntIdMovie(false, 99, null, getUTCDate(1959, 11, 16), "The 400 Blows", 1959));
        mAllIntIdMovies.add(new IntIdMovie(false, 83, null, getUTCDate(1967, 3, 16), "Persona", 1966));
        mAllIntIdMovies.add(new IntIdMovie(false, 106, null, getUTCDate(1967, 12, 22), "The Graduate", 1967));
        mAllIntIdMovies.add(new IntIdMovie(true, 191, "PG", getUTCDate(1983, 2, 25), "Gandhi", 1982));
        mAllIntIdMovies.add(new IntIdMovie(false, 85, null, getUTCDate(1956, 6, 6), "The Killing", 1956));
        mAllIntIdMovies.add(new IntIdMovie(false, 119, "PG", getUTCDate(2005, 6, 17), "Howl's Moving Castle", 2004));
        mAllIntIdMovies.add(new IntIdMovie(true, 100, "PG-13", getUTCDate(2012, 1, 20), "The Artist", 2011));
        mAllIntIdMovies.add(new IntIdMovie(false, 98, "PG", getUTCDate(1987, 9, 25), "The Princess Bride", 1987));
        mAllIntIdMovies.add(new IntIdMovie(false, 120, "R", getUTCDate(2012, 10, 12), "Argo", 2012));
        mAllIntIdMovies.add(new IntIdMovie(true, 120, "R", getUTCDate(2009, 1, 23), "Slumdog Millionaire", 2008));
        mAllIntIdMovies.add(new IntIdMovie(false, 131, null, getUTCDate(1966, 6, 22), "Who's Afraid of Virginia Woolf?", 1966));
        mAllIntIdMovies.add(new IntIdMovie(false, 108, "PG", getUTCDate(1956, 7, 16), "La Strada", 1954));
        mAllIntIdMovies.add(new IntIdMovie(false, 126, null, getUTCDate(1962, 10, 24), "The Manchurian Candidate", 1962));
        mAllIntIdMovies.add(new IntIdMovie(false, 134, null, getUTCDate(1961, 9, 25), "The Hustler", 1961));
        mAllIntIdMovies.add(new IntIdMovie(true, 135, "PG-13", getUTCDate(2002, 1, 4), "A Beautiful Mind", 2001));
        mAllIntIdMovies.add(new IntIdMovie(false, 145, "R", getUTCDate(1969, 6, 18), "The Wild Bunch", 1969));
        mAllIntIdMovies.add(new IntIdMovie(true, 119, "PG", getUTCDate(1976, 12, 3), "Rocky", 1976));
        mAllIntIdMovies.add(new IntIdMovie(false, 160, "TV-PG", getUTCDate(1959, 9, 1), "Anatomy of a Murder", 1959));
        mAllIntIdMovies.add(new IntIdMovie(false, 120, null, getUTCDate(1953, 8, 10), "Stalag 17", 1953));
        mAllIntIdMovies.add(new IntIdMovie(false, 122, "R", getUTCDate(1974, 3, 16), "The Exorcist", 1973));
        mAllIntIdMovies.add(new IntIdMovie(false, 138, "PG", getUTCDate(1972, 12, 10), "Sleuth", 1972));
        mAllIntIdMovies.add(new IntIdMovie(false, 80, null, getUTCDate(1948, 8, 28), "Rope", 1948));
        mAllIntIdMovies.add(new IntIdMovie(false, 184, "PG", getUTCDate(1975, 12, 18), "Barry Lyndon", 1975));
        mAllIntIdMovies.add(new IntIdMovie(false, 123, null, getUTCDate(1962, 4, 22), "The Man Who Shot Liberty Valance", 1962));
        mAllIntIdMovies.add(new IntIdMovie(false, 112, "R", getUTCDate(2009, 8, 14), "District 9", 2009));
        mAllIntIdMovies.add(new IntIdMovie(false, 163, null, getUTCDate(1980, 4, 17), "Stalker", 1979));
        mAllIntIdMovies.add(new IntIdMovie(false, 101, "R", getUTCDate(2002, 12, 12), "Infernal Affairs", 2002));
        mAllIntIdMovies.add(new IntIdMovie(false, 118, null, getUTCDate(1953, 9, 2), "Roman Holiday", 1953));
        mAllIntIdMovies.add(new IntIdMovie(false, 103, "PG", getUTCDate(1998, 6, 5), "The Truman Show", 1998));
        mAllIntIdMovies.add(new IntIdMovie(false, 111, "G", getUTCDate(2007, 6, 29), "Ratatouille", 2007));
        mAllIntIdMovies.add(new IntIdMovie(false, 143, "PG-13", getUTCDate(2003, 7, 9), "Pirates of the Caribbean: The Curse of the Black Pearl", 2003));
        mAllIntIdMovies.add(new IntIdMovie(false, 106, "R", getUTCDate(2008, 12, 12), "Ip Man", 2008));
        mAllIntIdMovies.add(new IntIdMovie(false, 112, "PG-13", getUTCDate(2007, 5, 23), "The Diving Bell and the Butterfly", 2007));
        mAllIntIdMovies.add(new IntIdMovie(false, 130, "PG-13", getUTCDate(2011, 7, 15), "Harry Potter and the Deathly Hallows: Part 2", 2011));
        mAllIntIdMovies.add(new IntIdMovie(false, 99, "R", getUTCDate(1967, 1, 18), "A Fistful of Dollars", 1964));
        mAllIntIdMovies.add(new IntIdMovie(false, 125, "PG", getUTCDate(1951, 12, 1), "A Streetcar Named Desire", 1951));
        mAllIntIdMovies.add(new IntIdMovie(false, 92, "G", getUTCDate(2001, 11, 2), "Monsters, Inc.", 2001));
        mAllIntIdMovies.add(new IntIdMovie(false, 133, "R", getUTCDate(1994, 2, 25), "In the Name of the Father", 1993));
        mAllIntIdMovies.add(new IntIdMovie(false, 127, "PG-13", getUTCDate(2009, 5, 8), "Star Trek", 2009));
        mAllIntIdMovies.add(new IntIdMovie(false, 84, "G", getUTCDate(1991, 11, 22), "Beauty and the Beast", 1991));
        mAllIntIdMovies.add(new IntIdMovie(false, 136, "R", getUTCDate(1968, 6, 12), "Rosemary's Baby", 1968));
        mAllIntIdMovies.add(new IntIdMovie(false, 104, null, getUTCDate(1950, 10, 13), "Harvey", 1950));
        mAllIntIdMovies.add(new IntIdMovie(false, 117, "PG", getUTCDate(1984, 3, 11), "Nauticaä of the Valley of the Wind", 1984));
        mAllIntIdMovies.add(new IntIdMovie(false, 109, "R", getUTCDate(2009, 1, 30), "The Wrestler", 2008));
        mAllIntIdMovies.add(new IntIdMovie(true, 133, null, getUTCDate(1930, 8, 24), "All Quiet on the Western Front", 1930));
        mAllIntIdMovies.add(new IntIdMovie(false, 98, null, getUTCDate(1996, 2, 23), "La Haine", 1995));
        mAllIntIdMovies.add(new IntIdMovie(true, 133, "R", getUTCDate(1988, 12, 16), "Rain Man", 1988));
        mAllIntIdMovies.add(new IntIdMovie(false, 66, null, getUTCDate(1925, 12, 24), "Battleship Potemkin", 1925));
        mAllIntIdMovies.add(new IntIdMovie(false, 138, "R", getUTCDate(2010, 2, 19), "Shutter Island", 2010));
        mAllIntIdMovies.add(new IntIdMovie(false, 81, null, getUTCDate(1929, 6, 3), "Nosferatu", 1922));
        mAllIntIdMovies.add(new IntIdMovie(false, 103, "R", getUTCDate(2003, 9, 19), "Spring, Summer, Fall, Winter... and Spring", 2003));
        mAllIntIdMovies.add(new IntIdMovie(false, 96, "R", getUTCDate(1979, 4, 25), "Manhattan", 1979));
        mAllIntIdMovies.add(new IntIdMovie(false, 138, "R", getUTCDate(2003, 10, 15), "Mystic River", 2003));
        mAllIntIdMovies.add(new IntIdMovie(false, 102, null, getUTCDate(1938, 2, 18), "Bringing Up Baby", 1938));
        mAllIntIdMovies.add(new IntIdMovie(false, 108, null, getUTCDate(1943, 1, 15), "Shadow of a Doubt", 1943));
        mAllIntIdMovies.add(new IntIdMovie(false, 125, "PG-13", getUTCDate(2004, 1, 9), "Big Fish", 2003));
        mAllIntIdMovies.add(new IntIdMovie(false, 124, "PG", getUTCDate(1986, 8, 2), "Castle in the Sky", 1986));
        mAllIntIdMovies.add(new IntIdMovie(false, 151, "PG", getUTCDate(1973, 12, 16), "Papillon", 1973));
        mAllIntIdMovies.add(new IntIdMovie(false, 76, "PG", getUTCDate(1993, 10, 29), "The Nightmare Before Christmas", 1993));
        mAllIntIdMovies.add(new IntIdMovie(false, 119, "R", getUTCDate(1987, 6, 3), "The Untouchables", 1987));
        mAllIntIdMovies.add(new IntIdMovie(false, 127, "PG-13", getUTCDate(1993, 6, 11), "Jurassic Park", 1993));
        mAllIntIdMovies.add(new IntIdMovie(false, 115, "R", getUTCDate(2008, 10, 24), "Let the Right One In", 2008));
        mAllIntIdMovies.add(new IntIdMovie(true, 109, null, getUTCDate(1967, 10, 14), "In the Heat of the Night", 1967));
        mAllIntIdMovies.add(new IntIdMovie(false, 170, "PG-13", getUTCDate(2009, 12, 24), "3 Idiots", 2009));
        mAllIntIdMovies.add(new IntIdMovie(false, 118, null, getUTCDate(1944, 9, 23), "Arsenic and Old Lace", 1944));
        mAllIntIdMovies.add(new IntIdMovie(false, 119, null, getUTCDate(1956, 3, 13), "The Searchers", 1956));
        mAllIntIdMovies.add(new IntIdMovie(false, 98, "PG", getUTCDate(2000, 9, 29), "In the Mood for Love", 2000));
        mAllIntIdMovies.add(new IntIdMovie(false, 141, null, getUTCDate(1959, 4, 4), "Rio Bravo", 1959));
    }

    private static List<StringIdMovie> mAllStringIdMovies;

    static {
        mAllStringIdMovies = new ArrayList<StringIdMovie>(mAllIntIdMovies.size());

        for (int i = 0; i < mAllIntIdMovies.size(); i++) {
            mAllStringIdMovies.add(new StringIdMovie(String.format("Movie %1$03d", i), mAllIntIdMovies.get(i)));
        }
    }

    public static IntIdMovie getRandomIntIdMovie(Random rndGen) {
        return mAllIntIdMovies.get(rndGen.nextInt(mAllIntIdMovies.size()));
    }

    public static StringIdMovie getRandomStringIdMovie(Random rndGen) {
        return mAllStringIdMovies.get(rndGen.nextInt(mAllStringIdMovies.size()));
    }

    public static List<IntIdMovie> getAllIntIdMovies() {
        return mAllIntIdMovies;
    }

    public static List<StringIdMovie> getAllStringIdMovies() {
        return mAllStringIdMovies;
    }
}