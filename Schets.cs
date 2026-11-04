using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

public class Schets
{
    public Bitmap bitmap;

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
        schetsobjecten.Clear();
    }
    public void Roteer()
    {
        bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
    }
    public void BitmapOpslaan(string naam)
    {   

        bitmap.Save(naam);  
    }

    public void LijstOpslaan(string naam)
    {
        string lijst2string = "";
        for (int i = 0; i < this.schetsobjecten.Count; i++)
        {
            lijst2string += schetsobjecten[i].toString();
            lijst2string += "\n";
        }
        StreamWriter writer = new StreamWriter(naam);
        writer.Write(lijst2string);
        writer.Close();
    }

    public void leesSchetsFile(string naam)
    {
        StreamReader reader = new StreamReader(naam);
        while (reader.ReadLine() != null)
        {
            string objectstring = reader.ReadLine();
            if (objectstring != null) schetsobjecten.Add(string2SchetsObject(objectstring));
        }
        reader.Close();
        TekenViaLijst();
    }

    private SchetsObject string2SchetsObject(string str)
    {
        string[] infoStringLijst = str.Split(' ');
        Point startpunt = new Point(int.Parse(infoStringLijst[1]), int.Parse(infoStringLijst[2]));
        Point eindpunt = new Point(int.Parse(infoStringLijst[3]), int.Parse(infoStringLijst[4]));
        Color kleur = Color.FromArgb(int.Parse(infoStringLijst[5]), int.Parse(infoStringLijst[6]), int.Parse(infoStringLijst[7]));
        switch (infoStringLijst[0])
        {
            case "tekst": return new TekstObject(infoStringLijst[8], startpunt, eindpunt, kleur);
            case "lijn": return new LijnObject(startpunt, eindpunt, kleur);
            case "volRechthoek": return new VolRechthoekObject(startpunt, eindpunt, kleur);
            case "randRechthoek": return new RandRechthoekObject(startpunt, eindpunt, kleur);
            case "schijf": return new SchijfObject(startpunt, eindpunt, kleur);
            case "ellips": return new EllipsObject(startpunt, eindpunt, kleur);
        }
        return null;
    }

}