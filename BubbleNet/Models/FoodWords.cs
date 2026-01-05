namespace BubbleNet.Models
{
    public static class FoodWords
    {
        // 255 single-word food names for IP octet mapping (1-255)
        public static readonly string[] Words = new string[]
        {
            "Apple",       // 1
            "Apricot",     // 2
            "Avocado",     // 3
            "Banana",      // 4
            "Bacon",       // 5
            "Bagel",       // 6
            "Basil",       // 7
            "Bean",        // 8
            "Beef",        // 9
            "Beet",        // 10
            "Berry",       // 11
            "Biscuit",     // 12
            "Blackberry",  // 13
            "Blueberry",   // 14
            "Bread",       // 15
            "Broccoli",    // 16
            "Brownie",     // 17
            "Butter",      // 18
            "Cabbage",     // 19
            "Cake",        // 20
            "Candy",       // 21
            "Cantaloupe",  // 22
            "Caramel",     // 23
            "Carrot",      // 24
            "Cashew",      // 25
            "Celery",      // 26
            "Cheese",      // 27
            "Cherry",      // 28
            "Chicken",     // 29
            "Chili",       // 30
            "Chips",       // 31
            "Chocolate",   // 32
            "Cilantro",    // 33
            "Cinnamon",    // 34
            "Citrus",      // 35
            "Clam",        // 36
            "Clove",       // 37
            "Cobbler",     // 38
            "Cocoa",       // 39
            "Coconut",     // 40
            "Coffee",      // 41
            "Cookie",      // 42
            "Corn",        // 43
            "Crab",        // 44
            "Cracker",     // 45
            "Cranberry",   // 46
            "Cream",       // 47
            "Crepe",       // 48
            "Croissant",   // 49
            "Cucumber",    // 50
            "Cumin",       // 51
            "Cupcake",     // 52
            "Curry",       // 53
            "Custard",     // 54
            "Date",        // 55
            "Dill",        // 56
            "Donut",       // 57
            "Duck",        // 58
            "Dumpling",    // 59
            "Eclair",      // 60
            "Egg",         // 61
            "Eggplant",    // 62
            "Endive",      // 63
            "Espresso",    // 64
            "Fennel",      // 65
            "Fig",         // 66
            "Fish",        // 67
            "Flan",        // 68
            "Flour",       // 69
            "Fondue",      // 70
            "Fritter",     // 71
            "Fudge",       // 72
            "Garlic",      // 73
            "Gelato",      // 74
            "Ginger",      // 75
            "Gnocchi",     // 76
            "Gouda",       // 77
            "Granola",     // 78
            "Grape",       // 79
            "Grapefruit",  // 80
            "Gravy",       // 81
            "Guava",       // 82
            "Gumbo",       // 83
            "Ham",         // 84
            "Hazelnut",    // 85
            "Herb",        // 86
            "Honey",       // 87
            "Hummus",      // 88
            "Icing",       // 89
            "Jam",         // 90
            "Jelly",       // 91
            "Jerky",       // 92
            "Kale",        // 93
            "Ketchup",     // 94
            "Kiwi",        // 95
            "Lamb",        // 96
            "Lasagna",     // 97
            "Leek",        // 98
            "Lemon",       // 99
            "Lentil",      // 100
            "Lettuce",     // 101
            "Lime",        // 102
            "Linguine",    // 103
            "Liver",       // 104
            "Lobster",     // 105
            "Lychee",      // 106
            "Macadamia",   // 107
            "Macaroni",    // 108
            "Mackerel",    // 109
            "Mango",       // 110
            "Maple",       // 111
            "Marshmallow", // 112
            "Mayo",        // 113
            "Meat",        // 114
            "Melon",       // 115
            "Milk",        // 116
            "Mint",        // 117
            "Miso",        // 118
            "Mocha",       // 119
            "Mochi",       // 120
            "Molasses",    // 121
            "Mousse",      // 122
            "Mozzarella",  // 123
            "Muffin",      // 124
            "Mushroom",    // 125
            "Mussel",      // 126
            "Mustard",     // 127
            "Mutton",      // 128
            "Naan",        // 129
            "Nacho",       // 130
            "Nectarine",   // 131
            "Noodle",      // 132
            "Nutmeg",      // 133
            "Oat",         // 134
            "Oatmeal",     // 135
            "Olive",       // 136
            "Omelet",      // 137
            "Onion",       // 138
            "Orange",      // 139
            "Oregano",     // 140
            "Oyster",      // 141
            "Paella",      // 142
            "Pancake",     // 143
            "Papaya",      // 144
            "Paprika",     // 145
            "Parsley",     // 146
            "Parsnip",     // 147
            "Pasta",       // 148
            "Pastry",      // 149
            "Pea",         // 150
            "Peach",       // 151
            "Peanut",      // 152
            "Pear",        // 153
            "Pecan",       // 154
            "Pepper",      // 155
            "Pesto",       // 156
            "Pickle",      // 157
            "Pie",         // 158
            "Pineapple",   // 159
            "Pistachio",   // 160
            "Pita",        // 161
            "Pizza",       // 162
            "Plum",        // 163
            "Popcorn",     // 164
            "Pork",        // 165
            "Potato",      // 166
            "Pretzel",     // 167
            "Prosciutto",  // 168
            "Prune",       // 169
            "Pudding",     // 170
            "Pumpkin",     // 171
            "Quiche",      // 172
            "Quinoa",      // 173
            "Radicchio",   // 174
            "Radish",      // 175
            "Raisin",      // 176
            "Ramen",       // 177
            "Raspberry",   // 178
            "Ravioli",     // 179
            "Relish",      // 180
            "Rhubarb",     // 181
            "Rice",        // 182
            "Ricotta",     // 183
            "Risotto",     // 184
            "Roll",        // 185
            "Rosemary",    // 186
            "Rum",         // 187
            "Rye",         // 188
            "Saffron",     // 189
            "Sage",        // 190
            "Salad",       // 191
            "Salami",      // 192
            "Salmon",      // 193
            "Salsa",       // 194
            "Salt",        // 195
            "Sardine",     // 196
            "Sauce",       // 197
            "Sausage",     // 198
            "Scallion",    // 199
            "Scallop",     // 200
            "Scone",       // 201
            "Seaweed",     // 202
            "Sesame",      // 203
            "Shallot",     // 204
            "Sherbet",     // 205
            "Shrimp",      // 206
            "Smoothie",    // 207
            "Snapper",     // 208
            "Sorbet",      // 209
            "Soup",        // 210
            "Sourdough",   // 211
            "Soy",         // 212
            "Spaghetti",   // 213
            "Spinach",     // 214
            "Squash",      // 215
            "Squid",       // 216
            "Steak",       // 217
            "Stew",        // 218
            "Strawberry",  // 219
            "Stuffing",    // 220
            "Sugar",       // 221
            "Sushi",       // 222
            "Syrup",       // 223
            "Taco",        // 224
            "Tahini",      // 225
            "Tamale",      // 226
            "Tamarind",    // 227
            "Tangerine",   // 228
            "Tapioca",     // 229
            "Tart",        // 230
            "Tea",         // 231
            "Tempeh",      // 232
            "Thyme",       // 233
            "Toast",       // 234
            "Tofu",        // 235
            "Tomato",      // 236
            "Tortilla",    // 237
            "Truffle",     // 238
            "Tuna",        // 239
            "Turkey",      // 240
            "Turmeric",    // 241
            "Turnip",      // 242
            "Vanilla",     // 243
            "Veal",        // 244
            "Vinegar",     // 245
            "Waffle",      // 246
            "Walnut",      // 247
            "Wasabi",      // 248
            "Watercress",  // 249
            "Watermelon",  // 250
            "Wheat",       // 251
            "Yam",         // 252
            "Yogurt",      // 253
            "Zest",        // 254
            "Zucchini"     // 255
        };

        /// <summary>
        /// Get the food word for an IP octet value (1-255)
        /// </summary>
        public static string GetWord(int octet)
        {
            if (octet < 1 || octet > 255)
                return "Unknown";
            return Words[octet - 1];
        }

        /// <summary>
        /// Get the IP octet value for a food word (1-255), returns -1 if not found
        /// </summary>
        public static int GetOctet(string word)
        {
            for (int i = 0; i < Words.Length; i++)
            {
                if (Words[i].Equals(word, System.StringComparison.OrdinalIgnoreCase))
                    return i + 1;
            }
            return -1;  // Return -1 for invalid words (not found)
        }

        /// <summary>
        /// Generate a word code from the last 3 octets of an IP address
        /// Format: Word1/Word2/Word3
        /// Note: Since we only have words for 1-255, octet 0 maps to word index 1 (Apple)
        /// </summary>
        public static string GenerateWordCode(byte octet2, byte octet3, byte octet4)
        {
            // Handle 0 values by treating them as 1 (minimum valid word index)
            int o2 = octet2 == 0 ? 1 : octet2;
            int o3 = octet3 == 0 ? 1 : octet3;
            int o4 = octet4 == 0 ? 1 : octet4;
            
            return $"{GetWord(o2)}/{GetWord(o3)}/{GetWord(o4)}";
        }

        /// <summary>
        /// Parse a word code back to IP octets
        /// Returns tuple of (octet2, octet3, octet4), returns (-1, -1, -1) if invalid
        /// </summary>
        public static (int octet2, int octet3, int octet4) ParseWordCode(string wordCode)
        {
            var parts = wordCode.Split('/');
            if (parts.Length != 3)
                return (-1, -1, -1);

            return (GetOctet(parts[0].Trim()), GetOctet(parts[1].Trim()), GetOctet(parts[2].Trim()));
        }

        /// <summary>
        /// Check if a word code is valid
        /// </summary>
        public static bool IsValidWordCode(string wordCode)
        {
            var octets = ParseWordCode(wordCode);
            return octets.octet2 > 0 && octets.octet3 > 0 && octets.octet4 > 0;
        }
    }
}
