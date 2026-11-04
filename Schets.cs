using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public class Schets
{
    private Bitmap bitmap;

    public List<SchetsObject> schetsobjecten;

        
    public Schets()
    {
        bitmap = new Bitmap(1, 1);
        schetsobjecten = new List<SchetsObject>();
    }
    public Graphics BitmapGraphics
    {
        get { return Graphics.FromImage(bitmap); }
    }
    public void VeranderAfmeting(Size sz)
    {
        if (sz.Width > bitmap.Size.Width || sz.Height > bitmap.Size.Height)
        {
            Bitmap nieuw = new Bitmap( Math.Max(sz.Width,  bitmap.Size.Width)
                                     , Math.Max(sz.Height, bitmap.Size.Height)
                                     );
            Graphics gr = Graphics.FromImage(nieuw);
            gr.FillRectangle(Brushes.White, 0, 0, sz.Width, sz.Height);
            gr.DrawImage(bitmap, 0, 0);
            bitmap = nieuw;
        }
    }
    public void Teken(Graphics gr)
    {
        gr.DrawImage(bitmap, 0, 0);
    }

    public void TekenViaLijst()
    {
        Bitmap nieuw = new Bitmap(this.bitmap.Size.Width
                                     , this.bitmap.Size.Height
                                     );
        Graphics gr = Graphics.FromImage(nieuw);
        gr.FillRectangle(Brushes.White, 0, 0, this.bitmap.Size.Width, this.bitmap.Size.Height);
        for (int i = 0; i < this.schetsobjecten.Count; i++) schetsobjecten[i].teken(gr);
        bitmap = nieuw;
    }

    public void Schoon()
    {
        Graphics gr = Graphics.FromImage(bitmap);
        gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
    }
    public void Roteer()
    {
        bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
    }
    public void opslaan(string naam)
    {
        bitmap.Save(naam);
    }
}