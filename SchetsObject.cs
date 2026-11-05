using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.Xml;
using System.Text;

public abstract class SchetsObject
{
    public Point startpunt;
    public Point eindpunt;
    public string type = "";

    public Color kleur;

    public int pendikte = 3;
    public abstract void teken(Graphics g);
    public abstract bool raak(SchetsControl s, Point p);

    public virtual string toString()
    {
        int x0 = startpunt.X;
        int y0 = startpunt.Y;
        int x1 = eindpunt.X;
        int y1 = eindpunt.Y;
        int r = kleur.R;
        int g = kleur.G;
        int b = kleur.B;
        return $"{type} {x0} {y0} {x1} {y1} {r} {g} {b} {pendikte}";
    }

    public virtual void roteer(Size sz)
    {
        int x0 = sz.Height - startpunt.Y;
        int y0 = startpunt.X;
        int x1 = sz.Height - eindpunt.Y;
        int y1 = eindpunt.X;
        startpunt = new Point(x0, y0);
        eindpunt = new Point(x1, y1);
    }

    public abstract SchetsObject Copy();
}

public class TekstObject : SchetsObject
{
    public string tekst;
    public int rotatieIn90Graden = 0;

    public Font font;

    public TekstObject(string s, Point startp, Point eindp, Color col)
    {
        TekstObjectInitialize(s, startp, eindp, col);
    }
    public TekstObject(string s, Point startp, Point eindp, Color col, int r)
    {
        TekstObjectInitialize(s, startp, eindp, col);
        rotatieIn90Graden = r;
    }

    public void TekstObjectInitialize(string s, Point startp, Point eindp, Color col)
    {
        tekst = s;
        this.startpunt = startp;
        this.eindpunt = eindp;
        this.kleur = col;
        this.font = new Font("Tahoma", 40);
        type = "tekst";
    }
    public override string toString()
    {
        string str = base.toString();
        return str + " " + $"{rotatieIn90Graden} " + tekst;
    }
    public override void teken(Graphics g)
    { 
        g.DrawString(tekst, font, new SolidBrush(kleur), this.startpunt, StringFormat.GenericTypographic);
    }

    public override bool raak(SchetsControl s, Point p)
    {
        Point pRotated = p;
        for (int j = 0; j < rotatieIn90Graden; j++)
        {
            pRotated = new Point(pRotated.Y, s.Schets.bitmap.Size.Height - pRotated.X);
        }
        return TweepuntTool.Punten2Rechthoek(startpunt, eindpunt).Contains(pRotated);
    }

    public override void roteer(Size sz)
    {
        rotatieIn90Graden++;
        rotatieIn90Graden %= 4;
    }

    public override SchetsObject Copy()
    {
        return new TekstObject(tekst, startpunt, eindpunt, kleur);
    }
}

public class LijnObject : SchetsObject //De hierarchie is hier anders! Voor alle tweepuntobjecten die geen lijnen zijn
    // wil je het liefst dat startpunt de linkerbovenhoek is en eindpunt de rechteronderhoek, daar is namelijk makkelijker mee te werken
    // maar bij lijnobject gaat dan waardevolle informatie verloren, dus die moet apart
{
    public LijnObject(Point p1, Point p2, Color col, int pdikte)
    {
        startpunt = p1;
        eindpunt = p2;
        kleur = col;
        type = "lijn";
        pendikte = pdikte;
    }

    public override void teken(Graphics g)
    {
        g.DrawLine(TweepuntTool.MaakPen(new SolidBrush(kleur), pendikte), startpunt, eindpunt);
    }

    public double afstandTotLijn(Point p) //tijd voor wiskunde! :D credits naar Wikipedia (altijd)
    {
        double dx = eindpunt.X - startpunt.X;
        double dy = eindpunt.Y - startpunt.Y;
        double c = eindpunt.X * startpunt.Y - eindpunt.Y * startpunt.X;
        return Math.Abs(dy * p.X - dx * p.Y + c) / Math.Sqrt(dy * dy + dx * dx);
    }
    public override bool raak(SchetsControl s, Point p)
    {
        return afstandTotLijn(p) < 5
            && p.X > Math.Min(startpunt.X - 5, eindpunt.X - 5) && p.X < Math.Max(startpunt.X + 5, eindpunt.X + 5)
            && p.Y > Math.Min(startpunt.Y - 5, eindpunt.Y - 5) && p.Y < Math.Max(startpunt.Y + 5, eindpunt.Y + 5);
        //deze laatste 4 checks zijn nodig om de afstand van het lijnSTUK tot het punt klein te houden, anders zou je ook raak klikken
        //als je 200 pixels in het verlengde van een lijn zou klikken, en dat is niet de bedoeling
    }
    public override SchetsObject Copy()
    {
        return new LijnObject(startpunt, eindpunt, kleur, pendikte);
    }

}
public abstract class VlakObject : SchetsObject
{
    public Point middelpunt;
    public VlakObject(Point p1, Point p2, Color col, int pdikte)
    {
        punten2StartEindPunten(p1, p2);
        kleur = col;
        pendikte = pdikte;
    }

    public void punten2StartEindPunten(Point p1, Point p2)
    {
        startpunt = new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
        eindpunt = new Point(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));
        middelpunt = new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
    }

    public override void roteer(Size sz)
    {
        base.roteer(sz);
        punten2StartEindPunten(startpunt, eindpunt);
    }
}

public class VolRechthoekObject : VlakObject
{
    public VolRechthoekObject(Point p1, Point p2, Color col, int pdikte) : base(p1, p2, col, pdikte)
    { type = "volRechthoek"; }
    public override void teken(Graphics g)
    {
        g.FillRectangle(new SolidBrush(kleur), TweepuntTool.Punten2Rechthoek(startpunt, eindpunt));
    }
    
    public override bool raak(SchetsControl s, Point p)
    {
        return TweepuntTool.Punten2Rechthoek(startpunt, eindpunt).Contains(p);
    }
    public override SchetsObject Copy()
    {
        return new VolRechthoekObject(startpunt, eindpunt, kleur, pendikte);
    }

}

public class RandRechthoekObject : VlakObject
{
    public RandRechthoekObject(Point p1, Point p2, Color col, int pdikte) : base(p1, p2, col, pdikte)
    { type = "randRechthoek"; }
    public override void teken(Graphics g)
    {
        g.DrawRectangle(TweepuntTool.MaakPen(new SolidBrush(kleur), pendikte), TweepuntTool.Punten2Rechthoek(startpunt, eindpunt)); //waarom moeilijk doen als je ook Jeroen's code kan hergebruiken :)
    }

    public override bool raak(SchetsControl s, Point p)
    {
        return TweepuntTool.Punten2Rechthoek(new Point(startpunt.X-5, startpunt.Y-5), new Point(eindpunt.X+5, eindpunt.Y+5)).Contains(p)
            && ! TweepuntTool.Punten2Rechthoek(new Point(startpunt.X + 5, startpunt.Y + 5), new Point(eindpunt.X - 5, eindpunt.Y - 5)).Contains(p);
    }
    public override SchetsObject Copy()
    {
        return new RandRechthoekObject(startpunt, eindpunt, kleur, pendikte);
    }
}

public class SchijfObject : VlakObject
{
    public SchijfObject(Point p1, Point p2, Color col, int pdikte) : base(p1, p2, col, pdikte)
    { type = "schijf"; }

    public override void teken(Graphics g)
    {
        g.FillEllipse(new SolidBrush(kleur), TweepuntTool.Punten2Rechthoek(startpunt, eindpunt));
    }

    public static bool puntInEllips(Point midp, Point eindp, Point p) //meer wiskunde :D ook van wikipedia, de reden dat deze functie een aparte static functie is, is om 'm
        //straks voor ellipsobject slim te kunnen  gebruiken :)
    {
        double cx = (p.X - midp.X) / (double)(eindp.X - midp.X);
        double cy = (p.Y - midp.Y) / (double)(eindp.Y - midp.Y);
        return cx*cx+ cy*cy <= 1;
    }

    public override bool raak(SchetsControl s, Point p)
    {
        return puntInEllips(this.middelpunt, this.eindpunt, p);
    }
    public override SchetsObject Copy()
    {
        return new SchijfObject(startpunt, eindpunt, kleur, pendikte);
    }
}

public class EllipsObject : VlakObject
{
    public EllipsObject(Point p1, Point p2, Color col, int pdikte) : base(p1, p2, col, pdikte)
    { type = "ellips";  }

    public override void teken(Graphics g)
    {
        g.DrawEllipse(TweepuntTool.MaakPen(new SolidBrush(kleur), pendikte), TweepuntTool.Punten2Rechthoek(startpunt, eindpunt));
    }

    public override bool raak(SchetsControl s, Point p)
    {
        return SchijfObject.puntInEllips(middelpunt, new Point(eindpunt.X + 5, eindpunt.Y + 5), p) 
            && !SchijfObject.puntInEllips(middelpunt, new Point(eindpunt.X - 5, eindpunt.Y - 5), p); //kijk en daarom heb ik puntInEllips gemaakt
    }
    public override SchetsObject Copy()
    {
        return new EllipsObject(startpunt, eindpunt, kleur, pendikte);
    }
}