namespace BubbleNet.Models
{
    /// <summary>
    /// Provides a mapping between IP address octets (1-255) and food words.
    /// This creates human-readable "word codes" like "Apple/Banana/Cherry" 
    /// that are easier to remember and share than IP addresses.
    /// 
    /// The word code format is: Word1/Word2/Word3 where each word maps to
    /// the last three octets of an IP address (x.octet2.octet3.octet4).
    /// </summary>
    public static class FoodWords
    {
        // ===== Food Word Dictionary =====
        // 255 single-word food names for IP octet mapping (1-255)
        // Each word corresponds to its index + 1 (e.g., Words[0] = "Apple" = octet value 1)
        public static readonly string[] Words = new string[]
        {
            "Apple",       // 1   - A common fruit, easy to remember
            "Apricot",     // 2   - Stone fruit
            "Avocado",     // 3   - Popular green fruit
            "Banana",      // 4   - Yellow curved fruit
            "Bacon",       // 5   - Cured pork product
            "Bagel",       // 6   - Ring-shaped bread
            "Basil",       // 7   - Aromatic herb
            "Bean",        // 8   - Legume
            "Beef",        // 9   - Red meat
            "Beet",        // 10  - Root vegetable
            "Berry",       // 11  - Small fruit category
            "Biscuit",     // 12  - Baked good
            "Blackberry",  // 13  - Dark berry
            "Blueberry",   // 14  - Blue berry
            "Bread",       // 15  - Staple food
            "Broccoli",    // 16  - Green vegetable
            "Brownie",     // 17  - Chocolate dessert
            "Butter",      // 18  - Dairy product
            "Cabbage",     // 19  - Leafy vegetable
            "Cake",        // 20  - Dessert
            "Candy",       // 21  - Sweet confection
            "Cantaloupe",  // 22  - Melon variety
            "Caramel",     // 23  - Sweet topping
            "Carrot",      // 24  - Orange root vegetable
            "Cashew",      // 25  - Tree nut
            "Celery",      // 26  - Crunchy vegetable
            "Cheese",      // 27  - Dairy product
            "Cherry",      // 28  - Red stone fruit
            "Chicken",     // 29  - Poultry
            "Chili",       // 30  - Spicy pepper
            "Chips",       // 31  - Fried snack
            "Chocolate",   // 32  - Sweet treat
            "Cilantro",    // 33  - Fresh herb
            "Cinnamon",    // 34  - Sweet spice
            "Citrus",      // 35  - Fruit category
            "Clam",        // 36  - Shellfish
            "Clove",       // 37  - Aromatic spice
            "Cobbler",     // 38  - Fruit dessert
            "Cocoa",       // 39  - Chocolate base
            "Coconut",     // 40  - Tropical fruit
            "Coffee",      // 41  - Caffeinated beverage
            "Cookie",      // 42  - Sweet baked snack
            "Corn",        // 43  - Yellow grain
            "Crab",        // 44  - Shellfish
            "Cracker",     // 45  - Crispy snack
            "Cranberry",   // 46  - Tart red berry
            "Cream",       // 47  - Dairy product
            "Crepe",       // 48  - Thin pancake
            "Croissant",   // 49  - French pastry
            "Cucumber",    // 50  - Green vegetable
            "Cumin",       // 51  - Earthy spice
            "Cupcake",     // 52  - Small cake
            "Curry",       // 53  - Spice blend
            "Custard",     // 54  - Creamy dessert
            "Date",        // 55  - Sweet dried fruit
            "Dill",        // 56  - Feathery herb
            "Donut",       // 57  - Ring-shaped pastry
            "Duck",        // 58  - Poultry
            "Dumpling",    // 59  - Filled dough
            "Eclair",      // 60  - Cream-filled pastry
            "Egg",         // 61  - Protein source
            "Eggplant",    // 62  - Purple vegetable
            "Endive",      // 63  - Leafy green
            "Espresso",    // 64  - Strong coffee
            "Fennel",      // 65  - Licorice-flavored plant
            "Fig",         // 66  - Sweet fruit
            "Fish",        // 67  - Seafood
            "Flan",        // 68  - Caramel dessert
            "Flour",       // 69  - Baking ingredient
            "Fondue",      // 70  - Melted cheese dish
            "Fritter",     // 71  - Fried dough
            "Fudge",       // 72  - Chocolate candy
            "Garlic",      // 73  - Pungent bulb
            "Gelato",      // 74  - Italian ice cream
            "Ginger",      // 75  - Spicy root
            "Gnocchi",     // 76  - Potato pasta
            "Gouda",       // 77  - Dutch cheese
            "Granola",     // 78  - Oat mixture
            "Grape",       // 79  - Vine fruit
            "Grapefruit",  // 80  - Citrus fruit
            "Gravy",       // 81  - Sauce
            "Guava",       // 82  - Tropical fruit
            "Gumbo",       // 83  - Cajun stew
            "Ham",         // 84  - Cured pork
            "Hazelnut",    // 85  - Tree nut
            "Herb",        // 86  - Aromatic plant
            "Honey",       // 87  - Sweet syrup
            "Hummus",      // 88  - Chickpea dip
            "Icing",       // 89  - Sweet frosting
            "Jam",         // 90  - Fruit spread
            "Jelly",       // 91  - Clear preserve
            "Jerky",       // 92  - Dried meat
            "Kale",        // 93  - Leafy green
            "Ketchup",     // 94  - Tomato condiment
            "Kiwi",        // 95  - Fuzzy fruit
            "Lamb",        // 96  - Sheep meat
            "Lasagna",     // 97  - Layered pasta
            "Leek",        // 98  - Onion relative
            "Lemon",       // 99  - Yellow citrus
            "Lentil",      // 100 - Small legume
            "Lettuce",     // 101 - Salad green
            "Lime",        // 102 - Green citrus
            "Linguine",    // 103 - Flat pasta
            "Liver",       // 104 - Organ meat
            "Lobster",     // 105 - Luxury shellfish
            "Lychee",      // 106 - Asian fruit
            "Macadamia",   // 107 - Hawaiian nut
            "Macaroni",    // 108 - Elbow pasta
            "Mackerel",    // 109 - Oily fish
            "Mango",       // 110 - Tropical fruit
            "Maple",       // 111 - Sweet syrup
            "Marshmallow", // 112 - Fluffy candy
            "Mayo",        // 113 - Creamy condiment
            "Meat",        // 114 - Protein category
            "Melon",       // 115 - Large fruit
            "Milk",        // 116 - Dairy beverage
            "Mint",        // 117 - Cool herb
            "Miso",        // 118 - Fermented paste
            "Mocha",       // 119 - Coffee + chocolate
            "Mochi",       // 120 - Rice cake
            "Molasses",    // 121 - Dark syrup
            "Mousse",      // 122 - Airy dessert
            "Mozzarella",  // 123 - Soft cheese
            "Muffin",      // 124 - Breakfast cake
            "Mushroom",    // 125 - Fungus
            "Mussel",      // 126 - Shellfish
            "Mustard",     // 127 - Yellow condiment
            "Mutton",      // 128 - Adult sheep meat
            "Naan",        // 129 - Indian bread
            "Nacho",       // 130 - Corn chip
            "Nectarine",   // 131 - Smooth peach
            "Noodle",      // 132 - Long pasta
            "Nutmeg",      // 133 - Warm spice
            "Oat",         // 134 - Grain
            "Oatmeal",     // 135 - Breakfast porridge
            "Olive",       // 136 - Oil fruit
            "Omelet",      // 137 - Egg dish
            "Onion",       // 138 - Pungent bulb
            "Orange",      // 139 - Citrus fruit
            "Oregano",     // 140 - Pizza herb
            "Oyster",      // 141 - Shellfish
            "Paella",      // 142 - Spanish rice
            "Pancake",     // 143 - Flat breakfast
            "Papaya",      // 144 - Tropical fruit
            "Paprika",     // 145 - Red spice
            "Parsley",     // 146 - Green garnish
            "Parsnip",     // 147 - White root
            "Pasta",       // 148 - Italian staple
            "Pastry",      // 149 - Baked dough
            "Pea",         // 150 - Green legume
            "Peach",       // 151 - Fuzzy fruit
            "Peanut",      // 152 - Ground nut
            "Pear",        // 153 - Bell-shaped fruit
            "Pecan",       // 154 - Pie nut
            "Pepper",      // 155 - Spice/vegetable
            "Pesto",       // 156 - Basil sauce
            "Pickle",      // 157 - Brined cucumber
            "Pie",         // 158 - Crust dessert
            "Pineapple",   // 159 - Tropical fruit
            "Pistachio",   // 160 - Green nut
            "Pita",        // 161 - Pocket bread
            "Pizza",       // 162 - Italian favorite
            "Plum",        // 163 - Purple fruit
            "Popcorn",     // 164 - Popped corn
            "Pork",        // 165 - Pig meat
            "Potato",      // 166 - Starchy tuber
            "Pretzel",     // 167 - Twisted bread
            "Prosciutto",  // 168 - Italian ham
            "Prune",       // 169 - Dried plum
            "Pudding",     // 170 - Creamy dessert
            "Pumpkin",     // 171 - Orange squash
            "Quiche",      // 172 - Egg pie
            "Quinoa",      // 173 - Grain-like seed
            "Radicchio",   // 174 - Bitter lettuce
            "Radish",      // 175 - Peppery root
            "Raisin",      // 176 - Dried grape
            "Ramen",       // 177 - Japanese noodles
            "Raspberry",   // 178 - Red berry
            "Ravioli",     // 179 - Stuffed pasta
            "Relish",      // 180 - Pickle condiment
            "Rhubarb",     // 181 - Tart stalk
            "Rice",        // 182 - Grain staple
            "Ricotta",     // 183 - Soft cheese
            "Risotto",     // 184 - Creamy rice
            "Roll",        // 185 - Small bread
            "Rosemary",    // 186 - Pine-like herb
            "Rum",         // 187 - Sugarcane spirit
            "Rye",         // 188 - Dark grain
            "Saffron",     // 189 - Expensive spice
            "Sage",        // 190 - Savory herb
            "Salad",       // 191 - Mixed greens
            "Salami",      // 192 - Cured sausage
            "Salmon",      // 193 - Pink fish
            "Salsa",       // 194 - Tomato dip
            "Salt",        // 195 - Essential seasoning
            "Sardine",     // 196 - Small fish
            "Sauce",       // 197 - Liquid topping
            "Sausage",     // 198 - Ground meat
            "Scallion",    // 199 - Green onion
            "Scallop",     // 200 - Shellfish
            "Scone",       // 201 - British pastry
            "Seaweed",     // 202 - Ocean plant
            "Sesame",      // 203 - Tiny seed
            "Shallot",     // 204 - Mild onion
            "Sherbet",     // 205 - Frozen dessert
            "Shrimp",      // 206 - Small shellfish
            "Smoothie",    // 207 - Blended drink
            "Snapper",     // 208 - White fish
            "Sorbet",      // 209 - Fruit ice
            "Soup",        // 210 - Liquid meal
            "Sourdough",   // 211 - Tangy bread
            "Soy",         // 212 - Bean product
            "Spaghetti",   // 213 - Long pasta
            "Spinach",     // 214 - Iron-rich green
            "Squash",      // 215 - Gourd vegetable
            "Squid",       // 216 - Tentacled seafood
            "Steak",       // 217 - Beef cut
            "Stew",        // 218 - Slow-cooked dish
            "Strawberry",  // 219 - Red berry
            "Stuffing",    // 220 - Bread filling
            "Sugar",       // 221 - Sweet crystal
            "Sushi",       // 222 - Japanese rice dish
            "Syrup",       // 223 - Sweet liquid
            "Taco",        // 224 - Mexican folded
            "Tahini",      // 225 - Sesame paste
            "Tamale",      // 226 - Corn husk dish
            "Tamarind",    // 227 - Sour fruit
            "Tangerine",   // 228 - Small orange
            "Tapioca",     // 229 - Starchy pearls
            "Tart",        // 230 - Small pie
            "Tea",         // 231 - Hot beverage
            "Tempeh",      // 232 - Fermented soy
            "Thyme",       // 233 - Small herb
            "Toast",       // 234 - Browned bread
            "Tofu",        // 235 - Bean curd
            "Tomato",      // 236 - Red fruit
            "Tortilla",    // 237 - Flat bread
            "Truffle",     // 238 - Luxury fungus
            "Tuna",        // 239 - Large fish
            "Turkey",      // 240 - Large poultry
            "Turmeric",    // 241 - Golden spice
            "Turnip",      // 242 - White root
            "Vanilla",     // 243 - Sweet flavor
            "Veal",        // 244 - Young beef
            "Vinegar",     // 245 - Sour liquid
            "Waffle",      // 246 - Grid breakfast
            "Walnut",      // 247 - Brain-shaped nut
            "Wasabi",      // 248 - Japanese heat
            "Watercress",  // 249 - Peppery green
            "Watermelon",  // 250 - Summer fruit
            "Wheat",       // 251 - Flour grain
            "Yam",         // 252 - Sweet tuber
            "Yogurt",      // 253 - Cultured dairy
            "Zest",        // 254 - Citrus peel
            "Zucchini"     // 255 - Green squash
        };

        /// <summary>
        /// Gets the food word for a given IP octet value.
        /// </summary>
        /// <param name="octet">IP octet value (1-255)</param>
        /// <returns>The corresponding food word, or "Unknown" if out of range</returns>
        public static string GetWord(int octet)
        {
            // Validate octet is within valid range (1-255)
            if (octet < 1 || octet > 255)
                return "Unknown";

            // Return word at index (octet - 1) since array is 0-indexed
            return Words[octet - 1];
        }

        /// <summary>
        /// Gets the IP octet value for a given food word.
        /// Performs case-insensitive matching.
        /// </summary>
        /// <param name="word">Food word to look up</param>
        /// <returns>IP octet value (1-255), or -1 if word not found</returns>
        public static int GetOctet(string word)
        {
            // Iterate through all words to find a case-insensitive match
            for (int i = 0; i < Words.Length; i++)
            {
                if (Words[i].Equals(word, System.StringComparison.OrdinalIgnoreCase))
                    return i + 1;  // Return 1-based octet value
            }
            return -1;  // Return -1 for invalid words (not found)
        }

        /// <summary>
        /// Generates a word code from the last 3 octets of an IP address.
        /// Format: Word1/Word2/Word3 (e.g., "Apple/Banana/Cherry")
        /// </summary>
        /// <param name="octet2">Second IP octet (x.THIS.x.x)</param>
        /// <param name="octet3">Third IP octet (x.x.THIS.x)</param>
        /// <param name="octet4">Fourth IP octet (x.x.x.THIS)</param>
        /// <returns>Word code string in format "Word1/Word2/Word3"</returns>
        /// <remarks>
        /// Since we only have words for 1-255, octet value 0 maps to word index 1 (Apple).
        /// This ensures all valid IP addresses can be represented.
        /// </remarks>
        public static string GenerateWordCode(byte octet2, byte octet3, byte octet4)
        {
            // Handle 0 values by treating them as 1 (minimum valid word index)
            // This is necessary because IP octets can be 0-255, but our word array is 1-255
            int o2 = octet2 == 0 ? 1 : octet2;
            int o3 = octet3 == 0 ? 1 : octet3;
            int o4 = octet4 == 0 ? 1 : octet4;

            // Build word code string with "/" separators
            return $"{GetWord(o2)}/{GetWord(o3)}/{GetWord(o4)}";
        }

        /// <summary>
        /// Parses a word code back to IP octets.
        /// Accepts flexible input formats for user convenience.
        /// </summary>
        /// <param name="wordCode">Word code to parse</param>
        /// <returns>Tuple of (octet2, octet3, octet4). Value of -1 means octet was not specified.</returns>
        /// <remarks>
        /// Accepts 1-3 segments separated by '/' or '.'; segments map to the last octets.
        /// Examples:
        ///   "Apple" => ( -1, -1, 1 )  (only octet4 specified)
        ///   "4.Apple" => ( -1, 4, 1 )
        ///   "3.4.Apple" => ( 3, 4, 1 )
        ///   "Apple/Banana/Cherry" => (1, 4, 28)
        /// Returns (-1,-1,-1) for invalid input or unknown words.
        /// </remarks>
        public static (int octet2, int octet3, int octet4) ParseWordCode(string wordCode)
        {
            // Handle null or empty input
            if (string.IsNullOrWhiteSpace(wordCode))
                return (-1, -1, -1);

            // Split on both '/' and '.' for flexible input
            var parts = wordCode.Split(new[] { '/', '.' }, System.StringSplitOptions.RemoveEmptyEntries);

            // Validate we have 1-3 parts
            if (parts.Length < 1 || parts.Length > 3)
                return (-1, -1, -1);

            // Initialize result array with -1 (unspecified) values
            int[] result = new int[] { -1, -1, -1 };

            // Calculate offset to align parts to the last octets
            // e.g., 1 part -> offset 2 (fills index 2), 2 parts -> offset 1 (fills indices 1,2)
            int offset = 3 - parts.Length;

            // Process each part
            for (int i = 0; i < parts.Length; i++)
            {
                var token = parts[i].Trim();
                if (string.IsNullOrEmpty(token)) return (-1, -1, -1);

                // Try parsing as a numeric octet first
                if (int.TryParse(token, out var numeric))
                {
                    // Validate numeric value is within IP octet range
                    if (numeric < 0 || numeric > 255) return (-1, -1, -1);
                    result[offset + i] = numeric;
                }
                else
                {
                    // Try parsing as a food word
                    var oct = GetOctet(token);
                    if (oct == -1) return (-1, -1, -1);  // Unknown word
                    result[offset + i] = oct;
                }
            }

            return (result[0], result[1], result[2]);
        }

        /// <summary>
        /// Validates whether a word code is valid and can be parsed.
        /// </summary>
        /// <param name="wordCode">Word code to validate</param>
        /// <returns>True if at least one valid octet can be parsed from the word code</returns>
        public static bool IsValidWordCode(string wordCode)
        {
            var octets = ParseWordCode(wordCode);
            // At least one octet must be specified (i.e., > 0). Unspecified octets are -1.
            return (octets.octet2 > 0) || (octets.octet3 > 0) || (octets.octet4 > 0);
        }
    }
}
