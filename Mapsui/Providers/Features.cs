// Copyright 2010 - Paul den Dulk (Geodan)
//
// This file is part of SharpMap.
// Mapsui is free software; you can redistribute it and/or modify
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Providers
{
    public class Features : IFeatures
    {
        private readonly List<IFeature> _features = new List<IFeature>();
        
        public string PrimaryKey { get; private set; }

        public int Count
        {
            get { return _features.Count; }
        }

        public IFeature this[int index]
        {
            get { return _features[index]; }
        }

        public Features()
        {
            // Perhaps this constructor should get a dictionary parameter
            // to specify the name and type of the columns
        }

        public Features(IEnumerable<IFeature> features)
        {
            foreach (var feature in features)
            {
                Add(feature);
            }
        }

        public Features(string primaryKey)
        {
            PrimaryKey = primaryKey;
        }

        public IFeature New()
        {
            // At this point it is possible to initialize an improved version of
            // Feature with a specifed set of columns.
            return new Feature();
        }

        public void Add(IFeature feature)
        {
            _features.Add(feature);
        }

        public void AddRange(IEnumerable<IFeature> features)
        {
            foreach (var feature in features)
            {
                _features.Add(feature);
            }
        }

        public IEnumerator<IFeature> GetEnumerator()
        {
            return _features.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _features.GetEnumerator();
        }

        public void Delete(object id)
        {
            if (string.IsNullOrEmpty(PrimaryKey)) throw new Exception($"You need to set the {nameof(PrimaryKey)} to use the id");
            _features.Remove(_features.First(f => f[PrimaryKey].Equals(id)));
        }

        public void Delete(IFeature feature, Func<IFeature, IFeature, bool> compare = null)
        {
            if (compare == null)
            {
                if (!_features.Remove(feature)) throw new Exception("Feature not found");
            }
            else
            {
                var fea = _features.FirstOrDefault(f => compare(f, feature));
                if (!_features.Remove(fea))
                    throw new Exception("Feature not found");
            }
        }
        
        public void Clear()
        {
            _features.Clear();
        }
    }        
}

