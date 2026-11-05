using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

public class Schets
{
    public Bitmap bitmap;

    public List<SchetsObject> schetsobjecten;

    public Edit lastEdit;
    public Edit redoEdit;
    
    public Schets()
    {
        bitmap = new Bitmap(1, 1);
        schetsobjecten = new List<SchetsObject>();
        lastEdit = null; redoEdit = null;
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
        for (int i = 0; i < this.schetsobjecten.Count; i++)
        {
            if (schetsobjecten[i].type == "tekst")
            {
                for(int j = 0; j < 4-((TekstObject)schetsobjecten[i]).rotatieIn90Graden; j++) nieuw.RotateFlip(RotateFlipType.Rotate90FlipNone);
                schetsobjecten[i].teken(gr);
                for (int j = 0; j < ((TekstObject)schetsobjecten[i]).rotatieIn90Graden; j++) nieuw.RotateFlip(RotateFlipType.Rotate90FlipNone);
            }
            else schetsobjecten[i].teken(gr);
        }
        bitmap = nieuw;
    }

    public void Schoon()
    {
        Edit edit = new Edit(schetsobjecten, lastEdit);
        lastEdit = edit;
        redoEdit = null;
        Graphics gr = Graphics.FromImage(bitmap);
        gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
        schetsobjecten.Clear();
    }
    public void Roteer() //deze optie is zo storend hoe roteer je een string zonder al het andere ook te roteren??????????
    {
        Edit edit = new Edit(schetsobjecten, lastEdit);
        lastEdit = edit;
        redoEdit = null;
        bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
        foreach (SchetsObject sobj in schetsobjecten)
        {
            sobj.roteer(bitmap.Size);
        }
    }
    public void BitmapOpslaan(string naam)
    {   

        bitmap.Save(naam);  
    }

    public void Undo()
    {
        if (lastEdit != null) { lastEdit.RevertEdit(this);}
        TekenViaLijst();
    }

    public void Redo()
    {
        if (redoEdit != null) { redoEdit.RedoEdit(this); }
        TekenViaLijst();
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
        int pendikte = int.Parse(infoStringLijst[8]); //merk op: deze int kan een rotatie zijn (voor tekst) of een pendikte (alle andere objecten)
        switch (infoStringLijst[0])
        {
            case "tekst": return new TekstObject(infoStringLijst[10], startpunt, eindpunt, kleur, int.Parse(infoStringLijst[9]));
            case "lijn": return new LijnObject(startpunt, eindpunt, kleur, pendikte);
            case "volRechthoek": return new VolRechthoekObject(startpunt, eindpunt, kleur, pendikte);
            case "randRechthoek": return new RandRechthoekObject(startpunt, eindpunt, kleur, pendikte);
            case "schijf": return new SchijfObject(startpunt, eindpunt, kleur, pendikte);
            case "ellips": return new EllipsObject(startpunt, eindpunt, kleur, pendikte);
        }
        return null;
    }

}

public class Edit
{
    List<SchetsObject> schetsobjecten;
    Edit edit;
    public Edit(List<SchetsObject> sobjen,  Edit e)
    {
        edit = e;
        schetsobjecten = new List<SchetsObject>();
        for (int i=0; i<sobjen.Count; i++)
        {
            schetsobjecten.Add(sobjen[i].Copy());
        }
    }

    public void RevertEdit(Schets s)
    {
        Edit redoEdit = new Edit(s.schetsobjecten, s.redoEdit);
        s.redoEdit = redoEdit;
        s.lastEdit = edit;
        s.schetsobjecten.Clear();
        for (int i = 0; i < schetsobjecten.Count; i++)
        {
            s.schetsobjecten.Add(schetsobjecten[i].Copy());
        }
    }

    public void RedoEdit(Schets s)
    {
        s.redoEdit = edit;
        s.lastEdit = new Edit(s.schetsobjecten, s.lastEdit);
        s.schetsobjecten.Clear();
        for (int i = 0; i < schetsobjecten.Count; i++)
        {
            s.schetsobjecten.Add(schetsobjecten[i].Copy());
        }
    }
}