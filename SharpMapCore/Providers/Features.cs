// Copyright 2010 - Paul den Dulk (Geodan)
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharpMap.Geometries;

namespace SharpMap.Providers
{
    public interface IFeature
    {
        IGeometry Geometry { get; set; }
        object this[string key] { get; set; }
        IEnumerable<string> Fields { get; }
    }

    public interface IFeatures : IEnumerable<IFeature>
    {
        string PrimaryKey { get; }
        void Add(IFeature feature);
        IFeature New();
        void Delete(object id);
        void Clear();
    }

    public class Features : IFeatures
    {
        private readonly List<IFeature> features = new List<IFeature>();
        
        public string PrimaryKey { get; private set; }

        public int Count
        {
            get { return features.Count; }
        }

        public IFeature this[int index]
        {
            get { return features[index]; }
        }

        public Features()
        {
            //Perhaps this constructor should get a dictionary parameter
            //to specify the name and type of the columns
        }

        public Features(string primaryKey)
        {
            PrimaryKey = primaryKey;
        }

        public IFeature New()
        {
            //At this point it is possible to initialize an improved version of
            //Feature with a specifed set of columns.
            return new Feature();
        }

        public void Add(IFeature feature)
        {
            features.Add(feature);
        }

        public IEnumerator<IFeature> GetEnumerator()
        {
            return features.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return features.GetEnumerator();
        }

        public void Delete(object id)
        {
            if (string.IsNullOrEmpty(PrimaryKey)) throw new Exception("Primary key of Features was not set");
            features.First(f => f[PrimaryKey] == id);
        }

        public void Clear()
        {
            features.Clear();
        }

        public class Feature : IFeature
        {
            private readonly Dictionary<string, object> dictionary;

            public Feature()
            {
                dictionary = new Dictionary<string, object>();
            }

            public IGeometry Geometry { get; set; }
            
            public virtual object this[string key]
            {
                get { return dictionary.ContainsKey(key) ? dictionary[key] : null; }
                set { dictionary[key] = value; }
            }

            public IEnumerable<string> Fields
            {
                get { foreach (var key in dictionary.Keys) yield return key; }
            }
        }
    }        
}

