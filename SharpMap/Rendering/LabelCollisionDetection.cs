// Copyright 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System.Collections.Generic;

namespace SharpMap.Rendering
{
    /// <summary>
    /// Class defining delegate for label collision detection and static predefined methods
    /// </summary>
    public static class LabelCollisionDetection
    {
        #region Delegates

        /// <summary>
        /// Delegate method for filtering labels. Useful for performing custom collision detection on labels
        /// </summary>
        /// <param name="labels"></param>
        /// <returns></returns>
        public delegate void LabelFilterMethod(List<Label> labels);

        #endregion

        #region Label filter methods

        /// <summary>
        /// Simple and fast label collision detection.
        /// </summary>
        /// <param name="labels"></param>
        public static void SimpleCollisionDetection(List<Label> labels)
        {
            labels.Sort(); // sort labels by intersectiontests of labelbox
            //remove labels that intersect other labels
            for (int i = labels.Count - 1; i > 0; i--)
                if (labels[i].CompareTo(labels[i - 1]) == 0)
                {
                    if (labels[i].Priority == labels[i - 1].Priority) continue;

                    if (labels[i].Priority > labels[i - 1].Priority)
                        labels.RemoveAt(i - 1);
                    else
                        labels.RemoveAt(i);
                }
        }

        /// <summary>
        /// Thorough label collision detection.
        /// </summary>
        /// <param name="labels"></param>
        public static void ThoroughCollisionDetection(List<Label> labels)
        {
            labels.Sort(); // sort labels by intersectiontests of labelbox
            //remove labels that intersect other labels
            for (int i = labels.Count - 1; i > 0; i--)
            {
                if (!labels[i].Show) continue;
                for (int j = i - 1; j >= 0; j--)
                {
                    if (!labels[j].Show) continue;
                    if (labels[i].CompareTo(labels[j]) == 0)
                        if (labels[i].Priority >= labels[j].Priority)
                        {
                            labels[j].Show = false;
                            //labels.RemoveAt(j);
                            //i--;
                        }
                        else
                        {
                            labels[i].Show = false;
                            //labels.RemoveAt(i);
                            //i--;
                            break;
                        }
                }
            }
        }

        #endregion
    }
}