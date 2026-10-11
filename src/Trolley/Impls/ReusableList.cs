using System.Collections.Generic;

namespace Trolley;

public class ReusableList<T> : List<T>
{
    private int lastIndex = 0;

    public void Save() => this.lastIndex = this.Count;
    public void Restore()
    {
        if (this.Count > this.lastIndex)
            this.RemoveRange(this.lastIndex, this.Count - this.lastIndex);
    }
}