using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalonLorena
{
    class DataBaseHelper
    {
        static string path = "data.db";

        public class Columns
        {
            public const string id = "id";
            public const string name = "name";
            public const string relation = "ralation";
            public const string parentId = "parentId";
        }
    }
}
