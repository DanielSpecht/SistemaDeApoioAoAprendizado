using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PF2.Models
{
    public class Tree
    {
        public Node node;
        public List<Tree> sons;
        public Tree()
        { 
            this.sons = new List<Tree>();
        }
    }
}