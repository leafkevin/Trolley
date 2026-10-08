using System;
using System.Collections.Generic;

namespace Trolley;

public class ReusableList<T> : List<T>  
{
    private int lastIndex = 0;

    public void Save() => this.lastIndex = this.Count;
    public void Reset()
    {
        if (this.Count > this.lastIndex)
            this.RemoveRange(this.lastIndex, this.Count - this.lastIndex);
    }
    //public ReusableList<T> Clone()
    //{
    //    var result = new ReusableList<T>();
    //    this.ForEach(f => result.Add((T)f.Clone()));
    //    result.lastIndex = this.lastIndex;
    //    return result;
    //}
}