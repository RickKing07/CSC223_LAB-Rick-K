using System.Net.Mail;

namespace AST
{
    public class CFG : DiGraph<Statement>
    {
        public Statement? Start { get; set; } //Starting point of our digraph

    }
}