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
