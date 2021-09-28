using System;
using System.Collections.Generic;

namespace Mapsui.Utilities
{
    public class Performance
    {
        int _pos;
        int _maxValues;
        int _count;
        double _sum = 0;
        public double[] _drawingTimes;

        public Performance(int maxValues = 20)
        {
            if (maxValues <= 0)
                throw new ArgumentException("maxValues must not be equal or less 0");

            _maxValues = maxValues;
            _pos = 0;
            _drawingTimes = new double[_maxValues];
        }

        /// <summary>
        /// Counter for number of redraws of map
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// MaxValues of drawing times that are saved and used for mean value
        /// </summary>
        public int MaxValues => _maxValues;

        /// <summary>
        /// Mean value of all MaxValues drawing times
        /// </summary>
        public double Mean => _sum / _maxValues;

        /// <summary>
        /// Add next drawing time
        /// </summary>
        /// <param name="time"></param>
        public void Add(double time)
        {
            _sum = _sum - _drawingTimes[_pos] + time;
            _drawingTimes[_pos++] = time;
            _count++;

            if (_pos >= _maxValues)
                _pos = 0;
        }

        /// <summary>
        /// Time be used for the last drawing
        /// </summary>
        public double LastDrawingTime
        {
            get
            {
                int pos = _pos == 0 ? _maxValues - 1 : _pos - 1;

                return _drawingTimes[pos];
            }
        }

        /// <summary>
        /// Get list of all drawing times
        /// </summary>
        /// <remarks>
        /// First entry is the newest time
        /// </remarks>
        public List<double> DrawingTimes {
            get
            {
                List<double> result = new List<double>(_maxValues);
                int pos = _pos == 0 ? _maxValues - 1 : _pos - 1;

                while (pos != _pos)
                {
                    result.Add(_drawingTimes[pos]);
                    pos = pos <= 0 ? _maxValues - 1 : pos--;
                }

                return result;
            }
        } 
    }
}
