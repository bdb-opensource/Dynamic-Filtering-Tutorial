using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DynamicFilteringTutorial
{
    public class ReplaceVisitor : ExpressionVisitor
    {
        private readonly Expression _from;
        private readonly Expression _to;
        public ReplaceVisitor(Expression from, Expression to)
        {
            this._from = from;
            this._to = to;
        }
        public override Expression Visit(Expression node)
        {
            return node == this._from ? this._to : base.Visit(node);
        }
    }

}
