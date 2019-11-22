using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

// the below code is a solution to the Hacker Rank problem of "Array and simple queries" that
// can be found at:
// https://www.hackerrank.com/challenges/array-and-simple-queries/problem
// The solution was to use a Treap with implicit indexing

// the following resources used to construct and ultimately solve this problem
// https://cp-algorithms.com/data_structures/treap.html#toc-tgt-3
// https://leetcode.com/problems/find-median-from-data-stream/discuss/185096/c-advanced-data-structure-split-based-treap-solution
// https://www.quora.com/q/threadsiiithyderabad/Treaps-One-Tree-to-Rule-em-all-Part-1
// https://pastebin.com/zs55STkr
// https://codereview.stackexchange.com/questions/108184/c-treap-implementation
// https://github.com/stangelandcl/Cls.Treap/blob/master/treap/Program.cs

namespace Treap
{    
    class Solution
    {
        readonly static Random _random = new Random();

        public class Treap<T>
        {
            // readonly interface to pass back nodes that are not changeable by the
            // caller, so only internal methods make changes to the internal tree
            // structure
            public interface INode
            {
                int Priority { get; }
                int Key { get; }
                T Value { get; }
                Node Left { get; }
                Node Right { get; }
                int Size { get; }
            }

            [DebuggerDisplay("Key={Key}, Priority={Priority}")]
            public class Node : INode
            {
                // the randomly generated priority that ensures the tree is a heap
                public int Priority { get; set; }

                // key that we ultimately use for implicit indexing
                public int Key { get; set; }

                // user defined value that we are storing
                public T Value { get; set; }

                // our left and right nodes
                public Node Left { get; set; }
                public Node Right { get; set; }

                // the size of the sub-tree that this node is a parent of (default of 1 if no children)
                public int Size { get; set; }

                public Node(int key, T value, int priority, Node left, Node right, int size = 1)
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

                private static Node LeftRotate(Node root)
                {
                    Node temp = root.Right;
                    root.Right = temp.Left;
                    temp.Left = root;

                    // update the sub-tree sizes
                    root.UpdateSize();
                    temp.UpdateSize();

                    return temp;
                }

                private static Node RightRotate(Node root)
                {
                    Node temp = root.Left;
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

                public static Node Merge(Node left, Node right)
                {
                    if (left == null)
                    {
                        return right;
                    }

                    if (right == null)
                    {
                        return left;
                    }

                    Node newNode = null;

                    // need to keep our tree in a valid heap as well, where everything on the left should
                    // be less than what is on the right
                    if (left.Priority > right.Priority)
                    {
                        Node newRight = Merge(left.Right, right);
                        newNode = new Node(left.Key, left.Value, left.Priority, left.Left, newRight);
                    }
                    else
                    {
                        Node newLeft = Merge(left, right.Left);
                        newNode = new Node(right.Key, right.Value, right.Priority, newLeft, right.Right);
                    }

                    // update the size of the new node and the current node to account for any changes
                    newNode.UpdateSize();
                    return newNode;
                }

                // the last parameter, index, is used for implicit indexing to find a given key
                public void Split(bool implicitIndex, int key, T value, out Node left, out Node right, int index = 0)
                {
                    Node newTree = null;

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
                        left = new Node(Key, Value, Priority, Left, newTree);
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
                        right = new Node(Key, Value, Priority, newTree, Right);
                        right.UpdateSize();
                    }
                }

                public Node Add(int key, T value)
                {
                    // when adding a node we are not using an implicit index
                    Split(false, key, value, out Node left, out Node right);
                    Node m = new Node(key, value);

                    return Merge(Merge(left, m), right);
                }

                // if using implict indexes this may not work, as the tree could be in a state where the
                // binary tree logic may not be correct (e.g., values less than the current node on the left,
                // values greater than the current node on the right)
                public INode GetIndex(Node node, int index)
                {
                    INode foundNode = null;

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
                            foundNode = node as INode;
                        }
                        else if (index - leftSize == 0)
                        {
                            // handle the situation where our current index might also be what we are looking
                            // for, but only because the size of the left subtree is the exact same size as
                            // our index (so therefore there is no room to the left)

                            // return an INode so the caller cannot make changes to the internal tree structure
                            foundNode = node as INode;
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

                public static Node Delete(Node root, int key)
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
                                root = Node.RightRotate(root);
                            }
                            else
                            {
                                root = Node.LeftRotate(root);
                            }
                        }
                        else if (root.Left != null)
                        {
                            root = Node.RightRotate(root);
                        }
                        else if (root.Right != null)
                        {
                            root = Node.LeftRotate(root);
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

            public Node Root { get; private set; }

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
                    Node temp = Get(Root, key);
                    return temp != null ? temp.Value : default(T);
                }

                set
                {
                    Root = Add(key, value);
                }
            }

            private Node Get(Node root, int key)
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

            public void InOrder(Node node, Action<T> visit)
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
                Root = Node.Merge(left?.Root, right?.Root);
            }

            public void Split(bool implicitIndex, int key, T value, out Treap<T> left, out Treap<T> right)
            {
                Root.Split(implicitIndex, key, value, out Node tempLeft, out Node tempRight);

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

            public INode GetFirst()
            {
                // because we use zero based indexing, the first item will always
                // be at position zero
                return Root.GetIndex(Root, 0);
            }

            public INode GetLast()
            {
                // because we use zero based indexing, the last item will be at the
                // index of count - 1
                return Root.GetIndex(Root, Count - 1);
            }

            public Node Add(int key, T value)
            {
                if (Root == null)
                {
                    Root = new Node(key, value);
                    return Root;
                }

                return Root.Add(key, value);
            }

            public INode GetIndex(int index)
            {
                return Root.GetIndex(Root, index);
            }

            public Node Delete(int key)
            {
                if (Root == null)
                {
                    return null;
                }

                Root = Node.Delete(Root, key);
                return Root;
            }
        }

        static void Main(string[] args)
        {        
            string[] nm = Console.ReadLine().Split(' ');

            int n = Convert.ToInt32(nm[0]);

            int m = Convert.ToInt32(nm[1]);
            
            int[] values = Array.ConvertAll(Console.ReadLine().Split(' '), temp => Convert.ToInt32(temp));            

            int[][] queries = new int[m][];

            for (int index = 0; index < m; index++)
            {
                queries[index] = Array.ConvertAll(Console.ReadLine().Split(' '), temp => Convert.ToInt32(temp));                
            }

            Treap<int> tree = new Treap<int>();

            // place all items in the treap, adding each one as we go.  Because our treap has implicit indexing
            // all items will be inserted as such that we can do an in order traversal and get the array back
            // in 0 to N-1 order where N is the length of the array            
            for (int index = 0; index < values.Length; index++)
            {
                // insert with the key being the position, so that an in order traversal will result in the
                // array in the order it was entered in
                tree[index] = values[index];
            }

            for (int index = 0; index < m; index++)
            {
                // we use zero based indexing, so subtract one from whatever is provided
                int query = queries[index][0];
                int start = queries[index][1] - 1;
                int end = queries[index][2] - 1;

                // split the root into begin and end trees, storing the end tree into
                // a temporary Treap as we are going to split that again
                // split based upon the implicit index, which in this case is start - 1 as we are
                // using zero based indexing, and need to split on the node before the start value.
                // Consider the following example:
                // List = { 1, 2, 3, 4, 5, 6, 7, 8 }
                // If we want the values 2, 3, and 4 from the list then we would split on the first
                // node, giving us:
                // beginTree = { 1 }
                // tempTree = { 2, 3, 4, 5, 6, 7, 8 }
                tree.Split(true, start - 1, 0, out Treap<int> beginTree, out Treap<int> tempTree);

                // split based upon the implicit index, but also account for the fact that the tree we are doing a
                // split against is smaller given the nodes we took away at the front.  The remaining tree is either
                // the size of the beginTree, or taking the end value, subtracting the start value (accounting for
                // zero based indexes), and that new index being the proper place in the smaller tree to split on.
                // Again consider the following example:
                // List = { 1, 2, 3, 4, 5, 6, 7, 8 }
                // We want the values 2, 3, and 4 from the list, so the start value would be two, and the end value
                // would be four.  We already did a split on 1, which gave us the following:
                // leftTree = { 1 }
                // tempTree = { 2, 3, 4, 5, 6, 7, 8 }
                // We now need to calculate a different end value, which in this case is end minus start, or
                // 4 - 1, meaning we split on position three in the list, giving us:
                // middleTree = { 2, 3, 4 }
                // endTree = { 5, 6, 7, 8 }
                // At this point we can combine our trees in whatever order specified (front or back)
                tempTree.Split(true, end - start, 0, out Treap<int> middleTree, out Treap<int> endTree);

                // do the following based upon the query type
                // type 1: middle + begin + end
                // type 2: begin + end + middle
                if (query == 1)
                {
                    tree.Merge(beginTree, endTree);
                    tree.Merge(middleTree, tree);
                }
                else if (query == 2)
                {
                    tree.Merge(beginTree, endTree);
                    tree.Merge(tree, middleTree);
                }
            }

            // print the absolute value of the first minus the last element (if the tree
            // is empty this will fail, but all test input provided assumes a list of at
            // least length one)
            Console.WriteLine(Math.Abs(tree.GetFirst().Value - tree.GetLast().Value));

            List<int> arr = new List<int>();

            // print the present order of the list (an in-order traversal will give the current
            // positioning of all elements after merge and split operations)
            tree.InOrder(tree.Root, (value) => { arr.Add(value); });
            Console.WriteLine(string.Join(" ", arr));
        }
    }
}
