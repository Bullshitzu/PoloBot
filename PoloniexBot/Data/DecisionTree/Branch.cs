using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloniexBot.Data.DecisionTree {
    internal class Branch {

        public Tree parentTree;
        public Branch parent;

        public Branch child1;
        public Branch child2;

        public Branch (Branch parent, Tree parentTree) {
            this.parent = parent;
            this.parentTree = parentTree;
        }

        


    }
}
