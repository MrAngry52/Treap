using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treap
{
    // readonly interface to pass back nodes that are not changeable by the
    // caller, so only internal methods make changes to the internal tree
    // structure
    public interface INode<T>
    {
        int Priority { get; }
        int Key { get; }
        T Value { get; }
        Node<T> Left { get; }
        Node<T> Right { get; }
        int Size { get; }
    }
}
