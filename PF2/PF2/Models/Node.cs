//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PF2.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Node
    {
        public Node()
        {
            this.Node1 = new HashSet<Node>();
            this.Question = new HashSet<Question>();
            this.Quiz = new HashSet<Quiz>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Nullable<int> ParentId { get; set; }
    
        public virtual ICollection<Node> Node1 { get; set; }
        public virtual Node Node2 { get; set; }
        public virtual ICollection<Question> Question { get; set; }
        public virtual ICollection<Quiz> Quiz { get; set; }
    }
}
