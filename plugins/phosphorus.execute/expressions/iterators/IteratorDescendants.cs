/*
 * phosphorus five, copyright 2014 - Mother Earth, Jannah, Gaia
 * phosphorus five is licensed as mit, see the enclosed LICENSE file for details
 */

using System;
using System.Collections.Generic;
using phosphorus.core;

namespace phosphorus.execute.iterators
{
    /// <summary>
    /// return all descendants of previous iterator, or children, children of children, and so on recursively
    /// </summary>
    public class IteratorDescendants : Iterator
    {
        public override IEnumerable<Node> Evaluate {
            get {
                foreach (Node idxCurrent in Left.Evaluate) {
                    foreach (Node idxChild in ReturnChildren (idxCurrent)) {
                        yield return idxChild;
                    }
                }
            }
        }

        /*
         * recursively invoked for all descendant nodes
         */
        private IEnumerable<Node> ReturnChildren (Node idx)
        {
            foreach (Node idxChild in idx.Children) {
                yield return idxChild;
                foreach (Node idxChildChild in ReturnChildren (idxChild)) {
                    yield return idxChildChild;
                }
            }
        }
    }
}

