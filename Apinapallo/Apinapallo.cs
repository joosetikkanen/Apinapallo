using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

/// @author Joose Tikkanen
/// @version 5.4.2017
/// <summary>
/// Apinapallo-peli
/// </summary>
public class Apinapallo : PhysicsGame
{
    const double KENTANLEVEYS = 1024;
    const double KENTANKORKEUS = 768;
    const double RUUDUN_LEVEYS = KENTANLEVEYS / 19;
    const double RUUDUN_KORKEUS = KENTANKORKEUS / 31;

    private List<Label> valikonKohdat;
    private PhysicsObject pelaaja; // pallo jota pomputellaan
    private PhysicsObject alusta; // alusta jolla palloa pomputellaan
    private PhysicsObject alareuna; // kentän alareuna
    private Image[] palikat = LoadImages("palikka", "murtuvapalikka", "murtunutpalikka"); // palikoiden eri kestävyyksien kuvat
    private int alkutilanne = 0; // apumuuttujja jolla asetetaan peli alkutilanteeseen
    private bool peliAloitettu = false;
    private int avaimiaKeratty = 0;
    private int kenttaNro = 1;
    const double KIMMOISUUS = 1.0;
    const double PALLON_MIN_NOPEUS = 250;
    const double PALLON_MAX_NOPEUS = 500;
    const double PALLON_KORKEUS_ALUSTASTA = 11.3;
    const double ALUSTAN_LIIKUTUSVOIMA = 600.0;

    /// <summary>
    /// Jypelistä kopioitu ja sovellettu bugifixi 
    /// joka kiihdyttää ja jarruttaa palloa tarpeen mukaan.
    /// </summary>
    /// <param name="time"></param>
    protected override void Update(Time time)
    {
        if (pelaaja != null && Math.Abs(pelaaja.Velocity.Y) < PALLON_MIN_NOPEUS && alkutilanne > 0)
        {
            pelaaja.Velocity = new Vector(pelaaja.Velocity.X, (pelaaja.Velocity.Y * 1.1) + 1);
        }
        if (pelaaja != null && Math.Abs(pelaaja.Velocity.Y) > PALLON_MAX_NOPEUS)
        {
            pelaaja.Velocity = new Vector(pelaaja.Velocity.X, pelaaja.Velocity.Y * 0.9);
        }


        base.Update(time);
    }

    /// <summary>
    /// Pelin alkuvalikko, josta kutsutaan SeuraavaKentta-aliohjelmaa
    /// joka luo aluksi ensimmäisen kentän.
    /// </summary>
    public void Valikko()
    {
        IsPaused = true;
        //kenttaNro = 1;
        avaimiaKeratty = 0;
        Level.Size = new Vector(1024, 768);
        SetWindowSize(1024, 768);
        Level.Background.Color = Color.Brown;
        Level.Background.Image = LoadImage("cropattu isä");

        valikonKohdat = new List<Label>();

        Label otsikko = new Label("APINAPALLO");
        otsikko.Position = new Vector(0, 150);
        otsikko.Color = Color.DarkBrown;
        otsikko.TextColor = Color.Gold;
        valikonKohdat.Add(otsikko);


        Label kohta1 = new Label("Aloita uusi peli");
        kohta1.Position = new Vector(0, 40);
        kohta1.Color = Color.DarkBrown;
        kohta1.BorderColor = Color.Black;
        valikonKohdat.Add(kohta1);

        if (peliAloitettu)
        {
            Label kohta3 = new Label("Jatka");
            kohta3.Position = new Vector(0, 0);
            kohta3.Color = Color.DarkBrown;
            kohta3.BorderColor = Color.Black;
            valikonKohdat.Add(kohta3);
            Mouse.ListenOn(kohta3, MouseButton.Left, ButtonState.Pressed, JatkaPelia, null);
        }

        Label kohta2 = new Label("Lopeta peli");
        kohta2.Position = new Vector(0, -40);
        kohta2.Color = Color.DarkBrown;
        kohta2.BorderColor = Color.Black;
        valikonKohdat.Add(kohta2);

        foreach (Label valikonKohta in valikonKohdat)
        {
            Add(valikonKohta);
        }

        Mouse.ListenOn(kohta1, MouseButton.Left, ButtonState.Pressed, UusiPeli, null);
        Mouse.ListenOn(kohta2, MouseButton.Left, ButtonState.Pressed, Exit, null);
        Mouse.ListenMovement(1.0, ValikossaLiikkuminen, null);
    }

    private void UusiPeli()
    {
        kenttaNro = 1;
        SeuraavaKentta();
    }

    private void JatkaPelia()
    {
        IsPaused = false;
        foreach (Label valikonKohta in valikonKohdat)
        {
            valikonKohta.Destroy();
        }

        if (kenttaNro == 1) Level.Background.Image = LoadImage("tausta");
        else if (kenttaNro == 2) Level.Background.Image = LoadImage("tokataso");

    }

    /// <summary>
    /// Valikkotekstien värien määritys, kun kursorilla liikutaan
    /// alkuvalikossa (jypelin ohjeista).
    /// </summary>
    /// <param name="hiirenTila"></param>
    public void ValikossaLiikkuminen(AnalogState hiirenTila)
    {
        foreach (Label kohta in valikonKohdat)
        {
            if (Mouse.IsCursorOn(kohta))
            {
                kohta.TextColor = Color.Red;
            }
            else
            {
                kohta.TextColor = Color.Yellow;
            }
        }
    }

    /// <summary>
    /// Beginissä kutsutaan aliohjelmaa, joka luo alkuvalikon.
    /// </summary>
    public override void Begin()
    {
        Valikko();
    }

    /// <summary>
    /// Luodaan tilanteenmukainen kenttä ja asetetaan ohjaimet sekä törmäyskäsittelijät.
    /// </summary>
    public void SeuraavaKentta()
    {
        ClearAll();
        alkutilanne = 0;

        if (kenttaNro == 1) LuoKentta("taso1", "tausta");
        else if (kenttaNro == 2) LuoKentta("taso2", "tokataso");

        AsetaOhjaimet();

        AddCollisionHandler<PhysicsObject, Palikka>(pelaaja, PalikkaanTormays);
        AddCollisionHandler(pelaaja, "avain", AvaimenKerays);
        AddCollisionHandler(pelaaja, "poikanen", PoikasenKerays);
        IsPaused = false;

    }

    /// <summary>
    /// Pelin viimeisen maalin keräyskäsittelijä
    /// </summary>
    /// <param name="pelaaja"></param>
    /// <param name="poikanen"></param>
    public void PoikasenKerays(PhysicsObject pelaaja, PhysicsObject poikanen)
    {
        pelaaja.Destroy();
        poikanen.Destroy();
        Label tekstikentta = new Label("Voitit pelin!");
        tekstikentta.Color = Color.Brown;
        tekstikentta.TextColor = Color.Gold;
        tekstikentta.BorderColor = Color.Black;
        Add(tekstikentta);
    }

    /// <summary>
    /// Avainten keräyskäsittelijä
    /// </summary>
    /// <param name="pelaaja"></param>
    /// <param name="avain"></param>
    public void AvaimenKerays(PhysicsObject pelaaja, PhysicsObject avain)
    {
        avain.Destroy();
        avaimiaKeratty++;
        if (avaimiaKeratty == 3)
        {
            kenttaNro++;
            SeuraavaKentta();
        }
    }

    /// <summary>
    /// Palikkaan törmäämisen käsittelijä
    /// </summary>
    /// <param name="pelaaja"></param>
    /// <param name="palikka"></param>
    public void PalikkaanTormays(PhysicsObject pelaaja, Palikka palikka)
    {

        palikka.OtaVastaanOsuma(palikat);
        if (palikka.Osumat >= palikat.Length)
        {
            pelaaja.Hit(new Vector(pelaaja.Velocity.X, pelaaja.Velocity.Y * -1.1));
        }

    }

    /// <summary>
    /// Määritetään kentän koko, reunat, taustakuva ja loput oliot
    /// </summary>
    /// <param name="kenttaTiedostonNimi"></param>
    /// <param name="taustakuva"></param>
    public void LuoKentta(string kenttaTiedostonNimi, string taustakuva)
    {
        Level.Size = new Vector(1024, 768);
        SetWindowSize(1024, 768);

        PhysicsObject ylareuna = Level.CreateTopBorder();
        ylareuna.Restitution = KIMMOISUUS;
        ylareuna.KineticFriction = 0.0;
        PhysicsObject vasenReuna = Level.CreateLeftBorder();
        vasenReuna.Restitution = KIMMOISUUS;
        vasenReuna.KineticFriction = 0.0;
        PhysicsObject oikeaReuna = Level.CreateRightBorder();
        oikeaReuna.Restitution = KIMMOISUUS;
        oikeaReuna.KineticFriction = 0.0;
        alareuna = Level.CreateBottomBorder();
        alareuna.IsVisible = false;

        Level.Background.Image = LoadImage(taustakuva);

        TileMap ruudut = TileMap.FromLevelAsset(kenttaTiedostonNimi);
        ruudut.SetTileMethod('p', LuoPalikka);
        ruudut.SetTileMethod('a', LuoAlusta, "alusta");
        ruudut.SetTileMethod('b', LuoPelaaja, "cropattu isä");
        ruudut.SetTileMethod('c', LuoAvain, "key");
        ruudut.SetTileMethod('d', LuoPoikanen, "poikanen");
        ruudut.Optimize('a');
        ruudut.Optimize('b');
        ruudut.Optimize('d');
        ruudut.Execute(RUUDUN_LEVEYS, RUUDUN_KORKEUS);
    }

    /// <summary>
    /// Pelin viimeisen kerättävän maalin luonti
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    /// <param name="kuvanNimi"></param>
    public void LuoPoikanen(Vector paikka, double leveys, double korkeus, string kuvanNimi)
    {
        PhysicsObject poikanen = new PhysicsObject(leveys, korkeus);
        poikanen.Position = paikka;
        poikanen.Tag = "poikanen";
        poikanen.Image = LoadImage(kuvanNimi);
        Add(poikanen);
    }

    /// <summary>
    /// Kerättävien avainten luonti
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    /// <param name="kuvanNimi"></param>
    public void LuoAvain(Vector paikka, double leveys, double korkeus, string kuvanNimi)
    {
        PhysicsObject avain = new PhysicsObject(leveys, korkeus);
        avain.IgnoresCollisionResponse = true;
        avain.Position = paikka;
        avain.Tag = "avain";
        avain.Image = LoadImage(kuvanNimi);
        Add(avain);
    }

    /// <summary>
    /// "Pelaajan" eli pompoteltavan pallon luonti, sekä pelaajan elämien 
    /// määritys ja alareunaan törmäämisen käsittely(ja pelin loppuminen).
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    /// <param name="kuvanNimi"></param>
    public void LuoPelaaja(Vector paikka, double leveys, double korkeus, string kuvanNimi)
    {
        pelaaja = new PhysicsObject(RUUDUN_LEVEYS / 2.1, RUUDUN_KORKEUS / 1.1, Shape.Circle);
        pelaaja.CanRotate = true;
        pelaaja.Position = paikka;
        pelaaja.Restitution = KIMMOISUUS;
        pelaaja.KineticFriction = 1.0;
        pelaaja.Image = LoadImage(kuvanNimi);
        Add(pelaaja);

        IntMeter elamat = new IntMeter(3, 0, 3);

        //Alareunaan törmäys ja alkutilanteeseen palaminen
        AddCollisionHandler(pelaaja, delegate (PhysicsObject pelaaja, PhysicsObject kohde)
        {
            if (kohde == alareuna)
            {

                alkutilanne = 0;
                pelaaja.X = alusta.X;
                pelaaja.Y = alusta.Top + PALLON_KORKEUS_ALUSTASTA;
                pelaaja.Stop();

                elamat.Value--;
            }
        });
        elamat.LowerLimit += delegate
        {
            pelaaja.Destroy();
            Label tekstikentta = new Label("Hävisit pelin!");
            tekstikentta.Color = Color.Brown;
            tekstikentta.TextColor = Color.Red;
            tekstikentta.BorderColor = Color.Black;
            Add(tekstikentta);
            //Keyboard.Listen(Key.Escape, ButtonState.Pressed, Begin, "alusta");
        };
    }

    /// <summary>
    /// Rikottavien palikkaesteiden luonti. Palikoiden kestävyydet ja kuvanvaihdot
    /// määritelty omassa Palikka-luokassa
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    public void LuoPalikka(Vector paikka, double leveys, double korkeus)
    {
        Palikka palikka = new Palikka(leveys, korkeus, palikat);
        palikka.MakeStatic();
        palikka.Position = paikka;
        palikka.Restitution = KIMMOISUUS;
        palikka.KineticFriction = 0.0;
        Add(palikka);
    }

    /// <summary>
    /// Palloa pompottelevan alustan luonti
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    /// <param name="kuvanNimi"></param>
    public void LuoAlusta(Vector paikka, double leveys, double korkeus, string kuvanNimi)
    {
        alusta = PhysicsObject.CreateStaticObject(leveys, korkeus);
        alusta.Tag = "alusta";
        alusta.Position = paikka;
        alusta.Restitution = KIMMOISUUS;
        alusta.KineticFriction = 1.0;
        alusta.Image = LoadImage(kuvanNimi);
        Add(alusta);
    }

    /// <summary>
    /// Ohjainten asetus
    /// </summary>
    public void AsetaOhjaimet()
    {
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, Valikko, "Lopeta peli");

        Keyboard.Listen(Key.R, ButtonState.Pressed, SeuraavaKentta, "alusta");
        Keyboard.Listen(Key.Right, ButtonState.Down, LiikutaAlustaa, "Liikuta alustaa oikealle", alusta, new Vector(ALUSTAN_LIIKUTUSVOIMA, 0));
        Keyboard.Listen(Key.Right, ButtonState.Released, LiikutaAlustaa, null, alusta, Vector.Zero);
        Keyboard.Listen(Key.Left, ButtonState.Down, LiikutaAlustaa, "Liikuta alustaa vasemmalle", alusta, new Vector(-1 * ALUSTAN_LIIKUTUSVOIMA, 0));
        Keyboard.Listen(Key.Left, ButtonState.Released, LiikutaAlustaa, null, alusta, Vector.Zero);
        Keyboard.Listen(Key.Space, ButtonState.Pressed, AloitaPeli, "Peli käyntiin", pelaaja);

        //testauskoodi
        /*
       Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, null, pelaaja, new Vector(100, 0));
       Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, null, pelaaja, new Vector(-100, 0));
       Keyboard.Listen(Key.Down, ButtonState.Down, Liikuta, null, pelaaja, new Vector(0, -100));
       Keyboard.Listen(Key.Up, ButtonState.Down, Liikuta, null, pelaaja, new Vector(0, 100));*/

    }

    //testauskoodi
    /*
   public void Liikuta(PhysicsObject pelaaja, Vector liikutusvoima)
   {
       pelaaja.Hit(liikutusvoima);
   }*/


    /// <summary>
    /// Alustan liikutuksen käsittelijä ja pelaajapallon määritys
    /// alustan päälle alkutilanteessa
    /// </summary>
    /// <param name="alusta"></param>
    /// <param name="nopeus"></param>
    public void LiikutaAlustaa(PhysicsObject alusta, Vector nopeus)
    {
        if (alusta.Left < Level.Left && nopeus.X < 0)
        {
            alusta.Velocity = Vector.Zero;
            return;
        }
        if (alusta.Right > Level.Right && nopeus.X > 0)
        {
            alusta.Velocity = Vector.Zero;
            return;
        }
        alusta.Velocity = nopeus;
        if (alkutilanne < 1)
        {
            pelaaja.MakeStatic();
            pelaaja.Velocity = Vector.Zero;
            pelaaja.X = alusta.X;
            pelaaja.Y = alusta.Top + PALLON_KORKEUS_ALUSTASTA;
        }
    }

    /// <summary>
    /// Pelin käynnistyksen käsittelijä. Pallo lähtee
    /// yläviistoon siihen suuntaan johon alustaa liikutetaan.
    /// </summary>
    /// <param name="pelaaja"></param>
    public void AloitaPeli(PhysicsObject pelaaja)
    {
        int suuntakerroin = 1;

        if (alkutilanne < 1)
        {
            pelaaja.Mass = 1.0;
            if (alusta.Velocity.X < 0) suuntakerroin = -1;
            pelaaja.Hit(new Vector(180 * suuntakerroin, 300));
            alkutilanne++;
            peliAloitettu = true;
        }
    }

}
