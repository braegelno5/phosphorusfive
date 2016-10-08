/*
 * Phosphorus Five, copyright 2014 - 2016, Thomas Hansen, phosphorusfive@gmail.com
 * 
 * This file is part of Phosphorus Five.
 *
 * Phosphorus Five is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Phosphorus Five is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * If you cannot for some reasons use the Affero GPL license, Phosphorus
 * Five is also commercially available under Quid Pro Quo terms. Check 
 * out our website at http://gaiasoul.com for more details.
 */

using System.Collections.Generic;
using p5.exp;
using p5.core;
using p5.data.helpers;
using p5.exp.exceptions;

namespace p5.data
{
    /// <summary>
    ///     Class wrapping [delete-data]
    /// </summary>
    public static class Delete
    {
        /// <summary>
        ///     Deletes items from your database
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "delete-data")]
        public static void delete_data (ApplicationContext context, ActiveEventArgs e)
        {
            // Basic syntax checking
            if (!XUtil.IsExpression (e.Args.Value))
                throw new LambdaException ("[delete-data] requires an expression as its value", e.Args, context);

            // Making sure we clean up and remove all arguments passed in after execution
            using (new Utilities.ArgsRemover (e.Args)) {

                // Acquiring lock on database
                lock (Common.Lock) {

                    // Used to store how many items are actually affected
                    int affectedItems = 0;

                    // Looping through database matches and removing nodes while storing which files have been changed
                    var changed = new List<Node> ();
                    foreach (var idxDest in e.Args.Get<Expression> (context).Evaluate (context, Common.Database, e.Args)) {

                        // Figuring out which file Node updated belongs to, and storing in changed list
                        Common.AddNodeToChanges (idxDest.Node, changed);

                        // Setting value to null, which works if user chooses to delete "value", "name" and/or "node"
                        // Path and count though will throw an exception
                        idxDest.Value = null;

                        // Incrementing affected items
                        affectedItems += 1;
                    }

                    // Saving all affected files
                    Common.SaveAffectedFiles (context, changed);

                    // Returning number of affected items
                    e.Args.Value = affectedItems;
                }
            }
        }
    }
}