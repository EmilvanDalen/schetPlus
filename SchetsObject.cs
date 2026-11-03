using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography.Xml;
using System.Text;

public abstract class SchetsObject
{
    public Point startpunt;
    public Point eindpunt;

    public Brush kwast;
    public abstract void teken(Graphics g);
    public abstract bool raak(Point p);
}

public class TekstObject : SchetsObject
{
    public string tekst;

    public Font font;

    public TekstObject(string s, Point startp, Point eindp, Brush b)
    {
        tekst = s;
        this.startpunt = startp;
        this.eindpunt = eindp;
        this.kwast = b;
        this.font = new Font("Tahoma", 40);
    }
    public override void teken(Graphics g)
    { 
        g.DrawString(tekst, font, kwast, this.startpunt, StringFormat.GenericTypographic);
       
    }

    public override bool raak(Point p)
    {
        return TweepuntTool.Punten2Rechthoek(startpunt, eindpunt).Contains(p);
    }
}

public class LijnObject : SchetsObject //De hierarchie is hier anders! Voor alle tweepuntobjecten die geen lijnen zijn
    // wil je het liefst dat startpunt de linkerbovenhoek is en eindpunt de rechteronderhoek, daar is namelijk makkelijker mee te werken
    // maar bij lijnobject gaat dan waardevolle informatie verloren, dus die moet apart
{
    public LijnObject(Point p1, Point p2, Brush b)
    {
        startpunt = p1;
        eindpunt = p2;
        kwast = b;
    }

    public override void teken(Graphics g)
    {
        g.DrawLine(TweepuntTool.MaakPen(kwast, 3), startpunt, eindpunt);
    }

    public double afstandTotLijn(Point p) //tijd voor wiskunde! :D credits naar Wikipedia (altijd)
    {
        double dx = eindpunt.X - startpunt.X;
        double dy = eindpunt.Y - startpunt.Y;
        double c = eindpunt.X * startpunt.Y - eindpunt.Y * startpunt.X;
        return Math.Abs(dy * p.X - dx * p.Y + c) / Math.Sqrt(dy * dy + dx * dx);
    }
    public override bool raak(Point p) 
    {
        return afstandTotLijn(p) < 5
            && p.X > Math.Min(startpunt.X - 5, eindpunt.X - 5) && p.X < Math.Max(startpunt.X + 5, eindpunt.X + 5)
            && p.Y > Math.Min(startpunt.Y - 5, eindpunt.Y - 5) && p.Y < Math.Max(startpunt.Y + 5, eindpunt.Y + 5);
        //deze laatste 4 checks zijn nodig om de afstand van het lijnSTUK tot het punt klein te houden, anders zou je ook raak klikken
        //als je 200 pixels in het verlengde van een lijn zou klikken, en dat is niet de bedoeling
    }
}
public abstract class VlakObject : SchetsObject
{
    public Point middelpunt;
    public VlakObject(Point p1, Point p2, Brush b)
    {
        startpunt = new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
        eindpunt = new Point(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));
        middelpunt = new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        kwast = b;
    }
}

public class VolRechthoekObject : VlakObject
{
    public VolRechthoekObject(Point p1, Point p2, Brush b) : base(p1, p2, b)
    {}
    public override void teken(Graphics g)
    {
        g.FillRectangle(kwast, TweepuntTool.Punten2Rechthoek(startpunt, eindpunt));
    }
    
    public override bool raak(Point p)
    {
        return TweepuntTool.Punten2Rechthoek(startpunt, eindpunt).Contains(p);
    }

}

public class RandRechthoekObject : VlakObject
{
    public RandRechthoekObject(Point p1, Point p2, Brush b) : base(p1, p2, b)
    { }
    public override void teken(Graphics g)
    {
        g.DrawRectangle(TweepuntTool.MaakPen(kwast,3), TweepuntTool.Punten2Rechthoek(startpunt, eindpunt)); //waarom moeilijk doen als je ook Jeroen's code kan hergebruiken :)
    }

    public override bool raak(Point p)
    {
        return TweepuntTool.Punten2Rechthoek(new Point(startpunt.X-5, startpunt.Y-5), new Point(eindpunt.X+5, eindpunt.Y+5)).Contains(p)
            && ! TweepuntTool.Punten2Rechthoek(new Point(startpunt.X + 5, startpunt.Y + 5), new Point(eindpunt.X - 5, eindpunt.Y - 5)).Contains(p);
    }
}

public class SchijfObject : VlakObject
{
    public SchijfObject(Point p1, Point p2, Brush b) : base(p1, p2, b)
    { }

    public override void teken(Graphics g)
    {
        g.FillEllipse(kwast, TweepuntTool.Punten2Rechthoek(startpunt, eindpunt));
    }

    public static bool puntInEllips(Point midp, Point eindp, Point p) //meer wiskunde :D ook van wikipedia, de reden dat deze functie een aparte static functie is, is om 'm
        //straks voor ellipsobject slim te kunnen  gebruiken :)
    {
        double cx = (p.X - midp.X) / (double)(eindp.X - midp.X);
        double cy = (p.Y - midp.Y) / (double)(eindp.Y - midp.Y);
        return cx*cx+ cy*cy <= 1;
    }

    public override bool raak(Point p)
    {
        return puntInEllips(this.middelpunt, this.eindpunt, p);
    }
}

public class EllipsObject : VlakObject
{
    public EllipsObject(Point p1, Point p2, Brush b) : base(p1, p2, b)
    { }

    public override void teken(Graphics g)
    {
        g.DrawEllipse(TweepuntTool.MaakPen(kwast, 3), TweepuntTool.Punten2Rechthoek(startpunt, eindpunt));
    }

    public override bool raak(Point p)
    {
        return SchijfObject.puntInEllips(middelpunt, new Point(eindpunt.X + 5, eindpunt.Y + 5), p) 
            && !SchijfObject.puntInEllips(middelpunt, new Point(eindpunt.X - 5, eindpunt.Y - 5), p); //kijk en daarom heb ik puntInEllips gemaakt
    }
}