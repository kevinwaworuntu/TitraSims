using UnityEngine;
using System;

[Serializable]
public class TahapanData
{
    [Tooltip("Nama deskriptif tahapan (hanya untuk referensi di editor).")]
    public string namaTahapan;

    [Tooltip("Nama marker persis seperti di database Vuforia.")]
    public string namaMarkerVuforia;


    [Tooltip("Prefab atau objek Panel Info yang akan dimunculkan untuk informasi terkait marker ini.")]
    public GameObject panelInfo; // Penambahan untuk info popup

}
