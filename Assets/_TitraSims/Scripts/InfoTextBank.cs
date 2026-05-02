using TMPro;
using UnityEngine;
public static class InfoTextBank
{
    
    public struct InfoStyle
    {
        public TMP_FontAsset font;                 
        public float fontSize;                     
        public TextAlignmentOptions alignment;     
        public bool bold;                          
        public float lineSpacing;                  
        public float paragraphSpacing;             
        public float marginLeft, marginTop, marginRight, marginBottom;  
    }

    
    public static readonly string[] TBA = {
        
        @"Preparasi Alat dan Bahan\n
Alat:
- Beaker glass
- Botol coklat
- Botol pencuci
- Buret
- Corong kaca
- Erlenmeyer
- Gelas ukur
- Kaca arloji
- Labu ukur
- Pipet tetes
- Spatel
- Statif dan klem
- Timbangan analitik
\n
Bahan:
- Indikator kristal violet
- Larutan aseton
- Larutan asam asetat glasial
- Larutan anhidrida asetat
- Larutan benzene
- Larutan standar asam perklorat
- Serbuk kafein
- Serbuk baku primer kalium biftalat",

        
        @"Preparasi Buret\n
1. Pastikan buret kering sebelum digunakan!
2. Bilas buret dengan larutan standar asam perklorat!
3. Masukkan larutan standar asam perklorat pada buret!",

        
        @"Preparasi Larutan Baku Primer\n
1. Masukkan 700 mg kalium biftalat ke dalam Erlenmeyer!
2. Masukkan 50 mL asam asetat glasial ke dalam Erlenmeyer!
3. Aduk Erlenmeyer!",

        @" Preparasi Larutan Blanko\n
1. Masukkan 40 mL anhidrida asetat ke dalam Erlenmeyer!
2. Masukkan 80 mL benzene ke dalam Erlenmeyer!
3. Aduk Erlenmeyer!",

        @"Penimbangan Sampel\n
Timbang sebanyak 400 mg sampel kafein!",

        @"Preparasi Larutan Sampel\n
1. Masukkan sampel ke dalam Erlenmeyer!
2. Masukkan 40 mL anhidrida asetat ke dalam Erlenmeyer!
3. Masukkan 80 mL benzene ke dalam Erlenmeyer!
4. Larutkan sampel dengan mengaduk Erlenmeyer!",

        @"Penambahan Indikator\n
1. Ambil indikator kristal violet dengan pipet!
2. Tambahkan 2-3 tetes indikator kristal violet dalam masing-masing Erlenmeyer!",

        @"Simulasi Pembakuan Secara Triplo (3x)\n
1. Perhatikan dan catat volume awal buret sebelum titrasi pada activity book!
2. Lakukan titrasi terhadap larutan baku primer kalium biftalat!
3. Perhatikan dan catat volume akhir buret setelah titrasi pada activity book!",

        @"Simulasi Titrasi Blanko\n
1. Perhatikan dan catat volume awal buret sebelum titrasi pada activity book!
2. Lakukan titrasi terhadap larutan blanko!
3. Perhatikan dan catat volume akhir buret setelah titrasi pada activity book!",

        @"Simulasi Titrasi Sampel Secara Triplo (3x)\n
1. Perhatikan dan catat volume awal buret sebelum titrasi pada activity book!
2. Lakukan titrasi terhadap larutan sampel kafein!
3. Perhatikan dan catat volume akhir buret setelah titrasi pada activity book!"

    };

    
    public static readonly InfoStyle[] TBAStyle = {
    };

    
    public static readonly string[] TK = {
        
        @"Preparasi Alat dan Bahan\n
Alat:
- Beaker glass
- Botol coklat
- Buret
- Corong kaca
- Erlenmeyer
- Gelas ukur
- Kaca arloji
- Labu ukur
- Pipet tetes
- Spatel
- Statif dan klem
- Timbangan analitik
\n
Bahan:
- Indikator biru hidroksinaftol P (300 mg)
- Aquades
- larutan NaOH 1N
- larutan HCl 3N
- larutan standar EDTA 0,05M 
- Serbuk sampel CaCO<sub>3</sub>",

             
        @"Preparasi Buret\n
1. Bilas buret dengan larutan standar EDTA!
2. Masukkan larutan standar pada buret!",

             
        @"Preparasi Larutan Baku Primer\n
1. Masukkan 200 mg baku primer CaCO<sub>3</sub> ke dalam Erlenmeyer!
2. Masukkan 10 mL aquades ke dalam Erlenmeyer!
3. Masukkan 2 mL HCl 1 N ke dalam Erlenmeyer!
4. Aduk Erlenmeyer!
4. Masukkan 100 mL aquades ke dalam Erlenmeyer!
5. Masukkan 30 mL larutan standar EDTA melalui buret ke dalam Erlenmeyer!
6. Masukkan 15 mL NaOH 1N ke dalam Erlenmeyer!
7. Aduk Erlenmeyer!",

             
        @"Preparasi Larutan Blanko\n
1. Masukkan beberapa mL aquades ke dalam Erlenmeyer!
2. Masukkan beberapa tetes HCl 3N ke dalam Erlenmeyer!
3. Masukkan 100 mL aquades ke dalam Erlenmeyer!
4. Masukkan 15 mL NaOH 1N ke dalam Erlenmeyer!
5. Aduk Erlenmeyer!",

             
        @"Penimbangan Sampel\n
1. Timbang sebanyak 200 mg sampel CaCO<sub>3</sub>!",

             
        @"Preparasi Larutan Sampel\n
1. Masukkan sampel yang telah ditimbang ke dalam Erlenmeyer!
2. Masukkan beberapa tetes aquades ke dalam Erlenmeyer!
3. Masukkan beberapa tetes HCl 3N ke dalam Erlenmeyer!
4. Masukkan 100 mL aquades ke dalam Erlenmeyer!
5. Masukkan 15 mL NaOH 1N ke dalam Erlenmeyer!
6. Larutkan sampel dengan mengaduk Erlenmeyer!",

             
        @"Penambahan Indikator\n
1. Timbang sebanyak 300 mg indikator biru hidroksinfatol!
2. Tambahkan indikator biru hidroksinaftol yang telah ditimbang ke dalam Erlenmeyer!",

             
        @"Simulasi Pembakuan Secara Triplo (3x)\n
1. Perhatikan dan catat volume awal buret sebelum titrasi pada activity book!
2. Lakukan titrasi terhadap larutan baku primer CaCO<sub>3</sub>!
3. Perhatikan dan catat volume akhir buret setelah titrasi pada activity book!",

             
        @"Simulasi Titrasi Blanko\n
1. Perhatikan dan catat volume awal buret sebelum titrasi pada activity book!
2. Lakukan titrasi terhadap larutan blanko!
3. Perhatikan dan catat volume akhir buret setelah titrasi pada activity book!",

             
        @"Simulasi Titrasi Sampel Secara Triplo (3x)\n
1. Perhatikan dan catat volume awal buret sebelum titrasi pada activity book!
2. Lakukan titrasi terhadap larutan sampel CaCO<sub>3</sub>!
3. Perhatikan dan catat volume akhir buret setelah titrasi pada activity book!"
    };

    
    public static readonly InfoStyle[] TKStyle = {
    };
}
