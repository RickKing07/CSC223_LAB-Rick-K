using System.Net.Mail;
using AST;

namespace Optimizer
{
    public class CFG : DiGraph<Statement>
    {
        public Statement? Start { get; set; } //Starting point of our digraph


        public CFG()
        {
            this.Start = null; //call a null digraph?
        }
    }
}