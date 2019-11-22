using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treap
{
    public class Treap<T>
    {
        public Node<T> Root { get; private set; }

        public int Count
        {
            // the size of the Treap is always determined by the size of the root node
            get { return Root != null ? Root.Size : 0; }
        }

        public T this[int key]
        {
            get
            {
                // handle the fact the key may not exist, and therefore temp is null
                Node<T> temp = Get(Root, key);
                return temp != null ? temp.Value : default(T);
            }

            set
            {
                Root = Add(key, value);
            }
        }

        private Node<T> Get(Node<T> root, int key)
        {
            if (root == null)
            {
                return null;
            }

            if (key < root.Key)
            {
                return Get(root.Left, key);
            }

            if (key > root.Key)
            {
                return Get(root.Right, key);
            }

            return root;
        }

        public void Clear()
        {
            Root = null;
        }

        public void InOrder(Node<T> node, Action<T> visit)
        {
            if (node != null)
            {
                InOrder(node.Left, visit);
                visit(node.Value);
                InOrder(node.Right, visit);
            }
        }

        public void Merge(Treap<T> left, Treap<T> right)
        {
            // update the root to reflect the merged tree
            // account for the fact the left or right tree could be null
            Root = Node<T>.Merge(left?.Root, right?.Root);
        }

        public void Split(bool implicitIndex, int key, T value, out Treap<T> left, out Treap<T> right)
        {
            Root.Split(implicitIndex, key, value, out Node<T> tempLeft, out Node<T> tempRight);

            // return two new Treaps to show the trees we now have by updating the root to reflect the
            // nodes that were passed back in the call to Split
            if (tempLeft != null)
            {
                left = new Treap<T>()
                {
                    Root = tempLeft
                };
            }
            else
            {
                left = null;
            }

            if (tempRight != null)
            {
                right = new Treap<T>()
                {
                    Root = tempRight
                };
            }
            else
            {
                right = null;
            }
        }

        public INode<T> GetFirst()
        {
            // because we use zero based indexing, the first item will always
            // be at position zero
            return Root.GetIndex(Root, 0);
        }

        public INode<T> GetLast()
        {
            // because we use zero based indexing, the last item will be at the
            // index of count - 1
            return Root.GetIndex(Root, Count - 1);
        }

        public Node<T> Add(int key, T value)
        {
            if (Root == null)
            {
                Root = new Node<T>(key, value);
                return Root;
            }

            return Root.Add(key, value);
        }

        public INode<T> GetIndex(int index)
        {
            return Root.GetIndex(Root, index);
        }

        public Node<T> Delete(int key)
        {
            if (Root == null)
            {
                return null;
            }

            Root = Node<T>.Delete(Root, key);
            return Root;
        }
    }
}
