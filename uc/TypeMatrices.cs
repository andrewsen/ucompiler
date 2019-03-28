using System;
using System.Collections.Generic;

namespace Compiler
{
    public static class TypeMatrices
    {
        // Aliases
        private const DataTypes Char = DataTypes.Char;
        private const DataTypes UI08 = DataTypes.UI8;
        private const DataTypes SI08 = DataTypes.I8;
        private const DataTypes UI16 = DataTypes.UI16;
        private const DataTypes SI16 = DataTypes.I16;
        private const DataTypes UI32 = DataTypes.UI32;
        private const DataTypes SI32 = DataTypes.I32;
        private const DataTypes UI64 = DataTypes.UI64;
        private const DataTypes SI64 = DataTypes.I64;
        private const DataTypes F_64 = DataTypes.Double;
        private const DataTypes Strn = DataTypes.String;
        private const DataTypes Null = DataTypes.Null;
        private const DataTypes Bool = DataTypes.Bool;
        private const DataTypes Void = DataTypes.Void;
        private const DataTypes Objt = DataTypes.Object;
        private const DataTypes Arry = DataTypes.Array;
        private const DataTypes Clss = DataTypes.Class;

        /// <summary>
        /// Arithmetics operations (- * /)
        /// </summary>
        public static readonly DataTypes[,] BasicArithmeticMatrix = {
            /* L\R    Char, UI08, SI08, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Strn, Null, Bool, Void, Objt, Arry, Clss
            /*Char*/ {Char, Char, Char, UI16, SI16, UI32, SI32, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI08*/ {Char, UI08, UI08, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Null, Null, Null, Null, Null, Null, Null},
            /*SI08*/ {Char, UI08, SI08, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Null, Null, Null, Null, Null, Null, Null},
            /*UI16*/ {UI16, UI16, UI16, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Null, Null, Null, Null, Null, Null, Null},
            /*SI16*/ {SI16, SI16, SI16, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Null, Null, Null, Null, Null, Null, Null},
            /*UI32*/ {UI32, UI32, UI32, UI32, UI32, UI32, UI32, UI64, SI64, F_64, Null, Null, Null, Null, Null, Null, Null},
            /*SI32*/ {SI32, SI32, SI32, SI32, SI32, UI32, SI32, UI64, SI64, F_64, Null, Null, Null, Null, Null, Null, Null},
            /*UI64*/ {Null, UI64, UI64, UI64, UI64, UI64, UI64, UI64, UI64, F_64, Null, Null, Null, Null, Null, Null, Null},
            /*SI64*/ {Null, SI64, SI64, SI64, SI64, SI64, SI64, UI64, SI64, F_64, Null, Null, Null, Null, Null, Null, Null},
            /*F_64*/ {Null, F_64, F_64, F_64, F_64, F_64, F_64, F_64, F_64, F_64, Null, Null, Null, Null, Null, Null, Null},
            /*Strn*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Null*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Bool*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Void*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Objt*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Arry*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Clss*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
        };

        /// <summary>
        /// Arithmetics (%) and bitwise operations (&amp; | ^)
        /// </summary>
        public static readonly DataTypes[,] ModuloBitwiseArithmeticMatrix = {
            /* L\R    Char, UI08, SI08, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Strn, Null, Bool, Void, Objt, Arry, Clss
            /*Char*/ {Char, Char, Char, UI16, SI16, UI32, SI32, UI64, SI64, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI08*/ {Char, UI08, UI08, UI16, SI16, UI32, SI32, UI64, SI64, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI08*/ {Char, UI08, SI08, UI16, SI16, UI32, SI32, UI64, SI64, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI16*/ {UI16, UI16, UI16, UI16, SI16, UI32, SI32, UI64, SI64, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI16*/ {SI16, SI16, SI16, UI16, SI16, UI32, SI32, UI64, SI64, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI32*/ {UI32, UI32, UI32, UI32, UI32, UI32, UI32, UI64, SI64, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI32*/ {SI32, SI32, SI32, SI32, SI32, UI32, SI32, UI64, SI64, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI64*/ {UI64, UI64, UI64, UI64, UI64, UI64, UI64, UI64, UI64, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI64*/ {SI64, SI64, SI64, SI64, SI64, SI64, SI64, UI64, SI64, Null, Null, Null, Null, Null, Null, Null, Null},
            /*F_64*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Strn*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Null*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Bool*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Void*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Objt*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Arry*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Clss*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
        };

        /// <summary>
        /// Shift operations (&lt;&lt; &gt;&gt;)
        /// </summary>
        public static readonly DataTypes[,] ShiftsArithmeticMatrix = {
            /* L\R    Char, UI08, SI08, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Strn, Null, Bool, Void, Objt, Arry, Clss
            /*Char*/ {Char, Char, Char, UI16, SI16, UI32, SI32, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI08*/ {Char, UI08, UI08, UI16, SI16, UI32, SI32, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI08*/ {Char, UI08, SI08, UI16, SI16, UI32, SI32, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI16*/ {UI16, UI16, UI16, UI16, SI16, UI32, SI32, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI16*/ {SI16, SI16, SI16, UI16, SI16, UI32, SI32, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI32*/ {UI32, UI32, UI32, UI32, UI32, UI32, UI32, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI32*/ {SI32, SI32, SI32, SI32, SI32, UI32, SI32, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI64*/ {UI64, UI64, UI64, UI64, UI64, UI64, UI64, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI64*/ {SI64, SI64, SI64, SI64, SI64, SI64, SI64, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*F_64*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Strn*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Null*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Bool*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Void*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Objt*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Arry*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Clss*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
        };

        /// <summary>
        /// Addition operations (+)
        /// </summary>
        public static readonly DataTypes[,] AdditionArithmeticMatrix = {
            /* L\R    Char, UI08, SI08, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Strn, Null, Bool, Void, Objt, Arry, Clss
            /*Char*/ {Char, Char, Char, UI16, SI16, UI32, SI32, Null, Null, Null, Strn, Null, Null, Null, Null, Null, Null},
            /*UI08*/ {Char, UI08, UI08, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Strn, Null, Null, Null, Null, Null, Null},
            /*SI08*/ {Char, UI08, SI08, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Strn, Null, Null, Null, Null, Null, Null},
            /*UI16*/ {UI16, UI16, UI16, UI16, UI16, UI32, SI32, UI64, SI64, F_64, Strn, Null, Null, Null, Null, Null, Null},
            /*SI16*/ {SI16, SI16, SI16, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Strn, Null, Null, Null, Null, Null, Null},
            /*UI32*/ {UI32, UI32, UI32, UI32, UI32, UI32, UI32, UI64, SI64, F_64, Strn, Null, Null, Null, Null, Null, Null},
            /*SI32*/ {SI32, SI32, SI32, SI32, SI32, UI32, SI32, UI64, SI64, F_64, Strn, Null, Null, Null, Null, Null, Null},
            /*UI64*/ {Null, UI64, UI64, UI64, UI64, UI64, UI64, UI64, UI64, F_64, Strn, Null, Null, Null, Null, Null, Null},
            /*SI64*/ {Null, SI64, SI64, SI64, SI64, SI64, SI64, UI64, SI64, F_64, Strn, Null, Null, Null, Null, Null, Null},
            /*F_64*/ {Null, F_64, F_64, F_64, F_64, F_64, F_64, F_64, F_64, F_64, Strn, Null, Null, Null, Null, Null, Null},
            /*Strn*/ {Strn, Strn, Strn, Strn, Strn, Strn, Strn, Strn, Strn, Strn, Strn, Strn, Strn, Null, Strn, Strn, Strn},
            /*Null*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Strn, Null, Null, Null, Null, Null, Null},
            /*Bool*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Strn, Null, Null, Null, Null, Null, Null},
            /*Void*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Objt*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Strn, Null, Null, Null, Null, Null, Null},
            /*Arry*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Strn, Null, Null, Null, Null, Null, Null},
            /*Clss*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Strn, Null, Null, Null, Null, Null, Null},
        };

        /// <summary>
        /// Bitwise operations (&amp; | ^)
        /// </summary>
        public static readonly DataTypes[,] BitwiseArithmeticMatrix = {
            /* L\R    Char, UI08, SI08, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Strn, Null, Bool, Void, Objt, Arry, Clss
            /*Char*/ {Char, Char, Char, UI16, SI16, UI32, SI32, UI64, SI64, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI08*/ {Char, UI08, UI08, UI16, SI16, UI32, SI32, UI64, SI64, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI08*/ {Char, UI08, SI08, UI16, SI16, UI32, SI32, UI64, SI64, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI16*/ {UI16, UI16, UI16, UI16, UI16, UI32, SI32, UI64, SI64, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI16*/ {SI16, SI16, SI16, UI16, SI16, UI32, SI32, UI64, SI64, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI32*/ {UI32, UI32, UI32, UI32, UI32, UI32, UI32, UI64, SI64, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI32*/ {SI32, SI32, SI32, SI32, SI32, UI32, SI32, UI64, SI64, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI64*/ {UI64, UI64, UI64, UI64, UI64, UI64, UI64, UI64, UI64, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI64*/ {SI64, SI64, SI64, SI64, SI64, SI64, SI64, UI64, SI64, Null, Null, Null, Null, Null, Null, Null, Null},
            /*F_64*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Strn*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Null*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Bool*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Bool, Null, Null, Null, Null},
            /*Void*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Objt*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Arry*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Clss*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
        };

        /// <summary>
        /// Comparison operations (&lt; &lt;= &gt;= &gt;)
        /// </summary>
        public static readonly DataTypes[,] ComparisonMatrix = {
            /* L\R    Char, UI08, SI08, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Strn, Null, Bool, Void, Objt, Arry, Clss
            /*Char*/ {Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI08*/ {Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Null, Null, Null, Null, Null, Null, Null},
            /*SI08*/ {Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Null, Null, Null, Null, Null, Null, Null},
            /*UI16*/ {Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Null, Null, Null, Null, Null, Null, Null},
            /*SI16*/ {Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Null, Null, Null, Null, Null, Null, Null},
            /*UI32*/ {Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Null, Null, Null, Null, Null, Null, Null},
            /*SI32*/ {Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Null, Null, Null, Null, Null, Null, Null},
            /*UI64*/ {Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Null, Null, Null, Null, Null, Null, Null},
            /*SI64*/ {Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Null, Null, Null, Null, Null, Null, Null},
            /*F_64*/ {Null, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Null, Null, Null, Null, Null, Null, Null},
            /*Strn*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Null*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Bool*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Void*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Objt*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Arry*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Clss*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
        };

        /// <summary>
        /// Equality operations ( != == )
        /// </summary>
        public static readonly DataTypes[,] EqualityMatrix = {
            /* L\R    Char, UI08, SI08, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Strn, Null, Bool, Void, Objt, Arry, Clss
            /*Char*/ {Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI08*/ {Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Null, Null, Null, Null, Null, Null, Null},
            /*SI08*/ {Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Null, Null, Null, Null, Null, Null, Null},
            /*UI16*/ {Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Null, Null, Null, Null, Null, Null, Null},
            /*SI16*/ {Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Null, Null, Null, Null, Null, Null, Null},
            /*UI32*/ {Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Null, Null, Null, Null, Null, Null, Null},
            /*SI32*/ {Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Null, Null, Null, Null, Null, Null, Null},
            /*UI64*/ {Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Null, Null, Null, Null, Null, Null, Null},
            /*SI64*/ {Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Null, Null, Null, Null, Null, Null, Null},
            /*F_64*/ {Null, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Bool, Null, Null, Null, Null, Null, Null, Null},
            /*Strn*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Bool, Bool, Null, Null, Null, Null, Null},
            /*Null*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Bool, Null, Null, Null, Bool, Bool, Bool},
            /*Bool*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Bool, Null, Null, Null, Null},
            /*Void*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Objt*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Bool, Null, Null, Bool, Null, Null},
            /*Arry*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Bool, Null, Null, Null, Bool, Null},
            /*Clss*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Bool, Null, Null, Null, Null, Bool},
        };

        /// <summary>
        /// Logical operations (&amp;&amp; ||)
        /// </summary>
        public static readonly DataTypes[,] LogicalMatrix = {
            /* L\R    Char, UI08, SI08, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Strn, Null, Bool, Void, Objt, Arry, Clss
            /*Char*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI08*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI08*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI16*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI16*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI32*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI32*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI64*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI64*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*F_64*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Strn*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Null*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Bool*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Bool, Null, Null, Null, Null},
            /*Void*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Objt*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Arry*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Clss*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
        };

        /// <summary>
        /// Member access operation (.)
        /// </summary>
        public static readonly DataTypes[,] MemberAccessMatrix = {
            /* L\R    Char, UI08, SI08, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Strn, Null, Bool, Void, Objt, Arry, Clss
            /*Char*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI08*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI08*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI16*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI16*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI32*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI32*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI64*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI64*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*F_64*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Strn*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Null*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Bool*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Void*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Objt*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Arry*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Clss*/ {Char, UI08, SI08, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Strn, Null, Bool, Void, Objt, Arry, Clss},
        };

        /// <summary>
        /// Assignment operation (=)
        /// </summary>
        public static readonly DataTypes[,] AssignmentMatrix = {
            /* L\R    Char, UI08, SI08, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Strn, Null, Bool, Void, Objt, Arry, Clss
            /*Char*/ {Char, Char, Char, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI08*/ {Null, UI08, UI08, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI08*/ {Null, SI08, SI08, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI16*/ {UI16, UI16, UI16, UI16, UI16, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI16*/ {SI16, SI16, SI16, SI16, SI16, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI32*/ {UI32, UI32, UI32, UI32, UI32, UI32, UI32, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI32*/ {SI32, SI32, SI32, SI32, SI32, SI32, SI32, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*UI64*/ {UI64, UI64, UI64, UI64, UI64, UI64, UI64, UI64, UI64, Null, Null, Null, Null, Null, Null, Null, Null},
            /*SI64*/ {SI64, SI64, SI64, SI64, SI64, SI64, SI64, SI64, SI64, Null, Null, Null, Null, Null, Null, Null, Null},
            /*F_64*/ {F_64, F_64, F_64, F_64, F_64, F_64, F_64, F_64, F_64, F_64, Null, Null, Null, Null, Null, Null, Null},
            /*Strn*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Strn, Strn, Null, Null, Null, Null, Null},
            /*Null*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Bool*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Bool, Null, Null, Null, Null},
            /*Void*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Objt*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Objt, Null, Null, Objt, Null, Null},
            /*Arry*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Arry, Null, Null, Null, Arry, Null},
            /*Clss*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Clss, Null, Null, Null, Null, Clss},
        };

        /// <summary>
        /// Cast operation
        /// </summary>
        public static readonly DataTypes[,] ImplicitCastMatrix = {
            /* L\R    Char, UI08, SI08, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Strn, Null, Bool, Void, Objt, Arry, Clss
            /*Char*/ {Char, Char, Char, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Strn, Null, Null, Null, Null, Null, Null},
            /*UI08*/ {Char, UI08, UI08, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Strn, Null, Null, Null, Null, Null, Null},
            /*SI08*/ {Char, UI08, SI08, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Strn, Null, Null, Null, Null, Null, Null},
            /*UI16*/ {UI16, UI16, UI16, UI16, UI16, UI32, SI32, UI64, SI64, F_64, Strn, Null, Null, Null, Null, Null, Null},
            /*SI16*/ {SI16, SI16, SI16, UI16, SI16, UI32, SI32, UI64, SI64, F_64, Strn, Null, Null, Null, Null, Null, Null},
            /*UI32*/ {UI32, UI32, UI32, UI32, UI32, UI32, UI32, UI64, SI64, F_64, Strn, Null, Null, Null, Null, Null, Null},
            /*SI32*/ {SI32, SI32, SI32, SI32, SI32, UI32, SI32, UI64, SI64, F_64, Strn, Null, Null, Null, Null, Null, Null},
            /*UI64*/ {UI64, UI64, UI64, UI64, UI64, UI64, UI64, UI64, UI64, F_64, Strn, Null, Null, Null, Null, Null, Null},
            /*SI64*/ {SI64, SI64, SI64, SI64, SI64, SI64, SI64, UI64, SI64, F_64, Strn, Null, Null, Null, Null, Null, Null},
            /*F_64*/ {F_64, F_64, F_64, F_64, F_64, F_64, F_64, F_64, F_64, F_64, Strn, Null, Null, Null, Null, Null, Null},
            /*Strn*/ {Strn, Strn, Strn, Strn, Strn, Strn, Strn, Strn, Strn, Strn, Strn, Strn, Strn, Null, Strn, Strn, Strn},
            /*Null*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Strn, Null, Null, Null, Null, Null, Null},
            /*Bool*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Strn, Null, Null, Null, Null, Null, Null},
            /*Void*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Null},
            /*Objt*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Strn, Null, Null, Null, Null, Null, Null},
            /*Arry*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Strn, Null, Null, Null, Null, Null, Null},
            /*Clss*/ {Null, Null, Null, Null, Null, Null, Null, Null, Null, Null, Strn, Null, Null, Null, Null, Null, Null},
        };

        /// <summary>
        /// Logical NOT operation
        /// </summary>
        public static readonly DataTypes[] UnaryMinusVector = {
            /*Char*/ Char,
            /*UI08*/ SI16,
            /*SI08*/ SI08,
            /*UI16*/ UI32,
            /*SI16*/ SI16,
            /*UI32*/ SI64,
            /*SI32*/ SI32,
            /*UI64*/ Null,
            /*SI64*/ SI64,
            /*F_64*/ F_64,
            /*Strn*/ Null,
            /*Null*/ Null,
            /*Bool*/ Null,
            /*Void*/ Null,
            /*Objt*/ Null,
            /*Arry*/ Null,
            /*Clss*/ Null,
        };

        /// <summary>
        /// Logical NOT operation
        /// </summary>
        public static readonly DataTypes[] UnaryPlusVector = {
            /*Char*/ Char,
            /*UI08*/ UI08,
            /*SI08*/ SI08,
            /*UI16*/ UI16,
            /*SI16*/ SI16,
            /*UI32*/ UI32,
            /*SI32*/ SI32,
            /*UI64*/ UI64,
            /*SI64*/ SI64,
            /*F_64*/ F_64,
            /*Strn*/ Null,
            /*Null*/ Null,
            /*Bool*/ Null,
            /*Void*/ Null,
            /*Objt*/ Null,
            /*Arry*/ Null,
            /*Clss*/ Null,
        };

        /// <summary>
        /// Logical NOT operation
        /// </summary>
        public static readonly DataTypes[] NotVector = {
            /*Char*/ Null,
            /*UI08*/ Null,
            /*SI08*/ Null,
            /*UI16*/ Null,
            /*SI16*/ Null,
            /*UI32*/ Null,
            /*SI32*/ Null,
            /*UI64*/ Null,
            /*SI64*/ Null,
            /*F_64*/ Null,
            /*Strn*/ Null,
            /*Null*/ Null,
            /*Bool*/ Bool,
            /*Void*/ Null,
            /*Objt*/ Null,
            /*Arry*/ Null,
            /*Clss*/ Null,
        };


        /// <summary>
        /// Arithmetical increment, dectrement (++ --) and bitwise inversion (negation);
        /// </summary>
        public static readonly DataTypes[] InvIncDecVector = {
            /*Char*/ Char,
            /*UI08*/ UI08,
            /*SI08*/ SI08,
            /*UI16*/ UI16,
            /*SI16*/ SI16,
            /*UI32*/ UI32,
            /*SI32*/ SI32,
            /*UI64*/ UI64,
            /*SI64*/ SI64,
            /*F_64*/ Null,
            /*Strn*/ Null,
            /*Null*/ Null,
            /*Bool*/ Null,
            /*Void*/ Null,
            /*Objt*/ Null,
            /*Arry*/ Null,
            /*Clss*/ Null,
        };

        /// <summary>
        /// Lookup table for binary operations
        /// </summary>
        public static readonly Dictionary<OperationType, DataTypes[,]> OperationMatrixLUT = new Dictionary<OperationType, DataTypes[,]> {
            {OperationType.Add, AdditionArithmeticMatrix},
            {OperationType.Sub, BasicArithmeticMatrix},
            {OperationType.Mul, BasicArithmeticMatrix},
            {OperationType.Div, BasicArithmeticMatrix},
            {OperationType.Mod, ModuloBitwiseArithmeticMatrix},
            {OperationType.BinAnd, ModuloBitwiseArithmeticMatrix},
            {OperationType.BinOr, ModuloBitwiseArithmeticMatrix},
            {OperationType.Xor, ModuloBitwiseArithmeticMatrix},
            {OperationType.ShiftLeft, ShiftsArithmeticMatrix},
            {OperationType.ShiftRight, ShiftsArithmeticMatrix},
            {OperationType.And, LogicalMatrix},
            {OperationType.Or, LogicalMatrix},
            {OperationType.Equals, EqualityMatrix},
            {OperationType.NotEquals, EqualityMatrix},
            {OperationType.GreaterEquals, ComparisonMatrix},
            {OperationType.LowerEquals, ComparisonMatrix},
            {OperationType.Greater, ComparisonMatrix},
            {OperationType.Lower, ComparisonMatrix},
            {OperationType.MemberAccess, MemberAccessMatrix},
            {OperationType.Assign, AssignmentMatrix},
        };


        /// <summary>
        /// Lookup table for unary operations
        /// </summary>
        public static readonly Dictionary<OperationType, DataTypes[]> OperationVectorLUT = new Dictionary<OperationType, DataTypes[]> {
            {OperationType.Inv, InvIncDecVector},
            {OperationType.PreInc, InvIncDecVector},
            {OperationType.PreDec, InvIncDecVector},
            {OperationType.PostInc, InvIncDecVector},
            {OperationType.PostDec, InvIncDecVector},
            {OperationType.Not, NotVector},
            {OperationType.UnaryMinus, UnaryMinusVector},
            {OperationType.UnaryPlus, UnaryPlusVector},
        };
    }
}
