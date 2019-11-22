using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Treap
{
    [DebuggerDisplay("Key={Key}, Priority={Priority}")]
    public class Node<T> : INode<T>
    {
        // to generate priority values to maintain the heap
        readonly static Random _random = new Random();

        // the randomly generated priority that ensures the tree is a heap
        public int Priority { get; set; }

        // key that we ultimately use for implicit indexing
        public int Key { get; set; }

        // user defined value that we are storing
        public T Value { get; set; }

        // our left and right nodes
        public Node<T> Left { get; set; }
        public Node<T> Right { get; set; }

        // the size of the sub-tree that this node is a parent of (default of 1 if no children)
        public int Size { get; set; }

        public Node(int key, T value, int priority, Node<T> left, Node<T> right, int size = 1)
        {
            // user defined types that maintain our key (for the binary tree) and actual value
            Key = key;
            Value = value;

            // pointers to our left and right nodes
            Left = left;
            Right = right;

            // priority maintains the heap order
            Priority = priority;

            // the size of the left and right children plus this node, or the default
            Size = size;
        }

        // generate a random priority value to ensure we have a reasonably balanced heap
        public Node(int key, T value) : this(key, value, _random.Next(), null, null, 1)
        {
        }

        public Node() : this(0, default(T))
        {
        }

        private static Node<T> LeftRotate(Node<T> root)
        {
            Node<T> temp = root.Right;
            root.Right = temp.Left;
            temp.Left = root;

            // update the sub-tree sizes
            root.UpdateSize();
            temp.UpdateSize();

            return temp;
        }

        private static Node<T> RightRotate(Node<T> root)
        {
            Node<T> temp = root.Left;
            root.Left = temp.Right;
            temp.Right = root;

            // update the sub-tree sizes
            root.UpdateSize();
            temp.UpdateSize();

            return temp;
        }

        public void UpdateSize()
        {
            int leftSize = Left != null ? Left.Size : 0;
            int rightSize = Right != null ? Right.Size : 0;

            // total of left subtree, plus right subtree, plus one for the current node
            Size = leftSize + rightSize + 1;
        }

        public static Node<T> Merge(Node<T> left, Node<T> right)
        {
            if (left == null)
            {
                return right;
            }

            if (right == null)
            {
                return left;
            }

            Node<T> newNode = null;

            // need to keep our tree in a valid heap as well, where everything on the left should
            // be less than what is on the right
            if (left.Priority > right.Priority)
            {
                Node<T> newRight = Merge(left.Right, right);
                newNode = new Node<T>(left.Key, left.Value, left.Priority, left.Left, newRight);
            }
            else
            {
                Node<T> newLeft = Merge(left, right.Left);
                newNode = new Node<T>(right.Key, right.Value, right.Priority, newLeft, right.Right);
            }

            // update the size of the new node and the current node to account for any changes
            newNode.UpdateSize();
            return newNode;
        }

        // the last parameter, index, is used for implicit indexing to find a given key
        public void Split(bool implicitIndex, int key, T value, out Node<T> left, out Node<T> right, int index = 0)
        {
            Node<T> newTree = null;

            // calculate the size of the left subtree for implicit indexing
            int leftSize = Left != null ? Left.Size : 0;
            int currentKey = 0;

            // are we splitting a tree based upon the key, or an implicit index?  If we are splitting
            // based upon an implicit index then we compare against a current key, calculated by
            // looking at the size of the left node plus the index passed in from the previous
            // call.  If we are not looking by implict index then we look based upon the key value
            // provided compared against the key of the current node
            bool result;

            if (implicitIndex)
            {
                // calculate the implict index
                currentKey = leftSize + index;
                result = currentKey <= key;
            }
            else
            {
                // just compare based upon the key of the current node
                result = Key <= key;
            }

            // is the key in the current node smaller the key being passed in?  If so then we
            // need to add this node to the left tree, and split along the right
            if (result)
            {
                if (Right == null)
                {
                    right = null;
                }
                else
                {
                    // if we are splitting based upon an implicit index the index we calculate
                    // will matter, but if we are splitting based upon a key value it is ignored
                    Right.Split(implicitIndex, key, value, out newTree, out right, currentKey + 1);
                }

                // make sure we update our new left node with the correct subtree size
                left = new Node<T>(Key, Value, Priority, Left, newTree);
                left.UpdateSize();
            }
            else
            {
                if (Left == null)
                {
                    left = null;
                }
                else
                {
                    // if we are splitting based upon an implicit index the index we calculate
                    // will matter, but if we are splitting based upon a key value it is ignored
                    Left.Split(implicitIndex, key, value, out left, out newTree, index);
                }

                // make sure we update our new right node with the correct subtree size
                right = new Node<T>(Key, Value, Priority, newTree, Right);
                right.UpdateSize();
            }
        }

        public Node<T> Add(int key, T value)
        {
            // when adding a node we are not using an implicit index
            Split(false, key, value, out Node<T> left, out Node<T> right);
            Node<T> m = new Node<T>(key, value);

            return Merge(Merge(left, m), right);
        }

        public INode<T> GetIndex(Node<T> node, int index)
        {
            INode<T> foundNode = null;

            // traverse the tree until we find the index, returning that node so that
            // we can use the key to do a subsequent split.  As this Treap implements
            // implicit indexing we can use the subtree size stored (and maintained)
            // in each node to figure out our index.  Assume the index is one-based, which
            // is to say the first element in our tree is position one, not position zero
            if (node != null)
            {
                // if the index is less than or equal to the size of the left sub-tree, then
                // we should traverse down that path with the index unchanged
                int leftSize = node.Left != null ? node.Left.Size : 0;

                // if our current index is smaller than the size of the left subtree then continue
                // further down the left subtree to find the index we are looking for
                if (index < leftSize)
                {
                    foundNode = GetIndex(node.Left, index);
                }
                else if (node.Left != null && index == 0)
                {
                    // if our index is zero then we are looking for the left most node, which will be
                    // the index we want to return, but only if the node does not have any further
                    // children on the left

                    // return an INode so the caller cannot make changes to the internal tree structure
                    foundNode = node as INode<T>;
                }
                else if (index - leftSize == 0)
                {
                    // handle the situation where our current index might also be what we are looking
                    // for, but only because the size of the left subtree is the exact same size as
                    // our index (so therefore there is no room to the left)

                    // return an INode so the caller cannot make changes to the internal tree structure
                    foundNode = node as INode<T>;
                }
                else
                {
                    // go to the right subtree.  Take into account the fact we need to reset the index
                    // by subtracting the size of the left subtree plus one (to account for the current
                    // node as well)
                    foundNode = GetIndex(node.Right, index - (leftSize + 1));
                }
            }

            return foundNode;
        }

        public static Node<T> Delete(Node<T> root, int key)
        {
            if (root == null)
            {
                return null;
            }

            if (key < root.Key)
            {
                root.Left = Delete(root.Left, key);
            }

            if (key > root.Key)
            {
                root.Right = Delete(root.Right, key);
            }

            if (key == root.Key)
            {
                if (root.Left != null && root.Right != null)
                {
                    if (root.Left.Priority < root.Right.Priority)
                    {
                        root = Node<T>.RightRotate(root);
                    }
                    else
                    {
                        root = Node<T>.LeftRotate(root);
                    }
                }
                else if (root.Left != null)
                {
                    root = Node<T>.RightRotate(root);
                }
                else if (root.Right != null)
                {
                    root = Node<T>.LeftRotate(root);
                }
                else
                {
                    return null;
                }

                root = Delete(root, key);
            }

            // update the size of the root to reflect the removed node (if found)
            root?.UpdateSize();
            return root;
        }
    }
}
