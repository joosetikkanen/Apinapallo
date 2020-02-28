using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;

/// @author Joose Tikkanen
/// @version 5.4.2017
/// <summary>
/// Pelissä rikottavat palikat
/// </summary>
public class Palikka : PhysicsObject
{
    public Image[] Palikat { get; set; }
    private int Osumat { get; set; }

    /// <summary>
    /// Luo uuden palikan
    /// </summary>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    /// <param name="kuvat"></param>
    public Palikka(double leveys, double korkeus, Image[] kuvat)
        : base(leveys, korkeus)
    {
        Image[] Palikat = kuvat;

        Osumat = 0;
        this.Image = Palikat[0];
    }

    /// <summary>
    /// Palikoiden kestävyyden määritys ja niiden kuvan vaihto pallon osuessa niihin.
    /// Palikan rikkoutuessa pallon pysähtyminen estetään.
    /// </summary>
    /// <param name="kuvat"></param>
    public void OtaVastaanOsuma(Image[] kuvat)
    {
        Osumat++;
        if (Osumat == kuvat.Length - 1)
        {
            this.IgnoresCollisionResponse = true;
        }
        if (Osumat >= kuvat.Length)
        {
            this.Destroy();
            return;
        }
        this.Image = kuvat[Osumat];
    }

}