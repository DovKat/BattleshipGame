### Projekto aprašymas

Kuriamas projektas – 2D laivų mūšio žaidimas. Žaidime kovos 2 žaidėjai prieš 2 žaidėjus, komandooje esantys neriai turės savo atskyra "lauką" kuriame bus sudėti jo pasirinkti laivai, taigi išviso žaidimas susidarys iš 4 laukų. Žaidimo pradžioje žaidėjams bus duotas tam tikras laivų skaičius, visi žaidėjai gaus tiek pat, tokių pačių laivų. Kiekvienas etapas susidarys iš keturių ėjimų, po vieną kiekvienam žaidėjui. Pralaimėjimus vienam žaidėjui, nepralaimi visa komanda, paskutinis likęs komandos narys gali žaisti toliau. Žaidimas laimimas kai priešininkų komandos visi laivai buvo surasti ir sunaikinti. 
Žaidėjai galės naudoti kelių skirtingų tipų šovinius ir laivų pagerinimus. Žaidėjas galės valdyti viską su pele arba klaviatūros klavišais norint pasukti laivą. 

Komandos nariai:
- Matas Asačiovas
- Tomas Jasulevičius
- Matas Pagalys
- Dovydas Katinas

### Naudojamos technologijos
- Komunikacija tarp žaidėjų: SignalR
- Programavimo kalbos: JavaScript, C#


### Funkciniai reikalavimai

1. Prisijungimas:

- Žaidėjas gali prisijungti prie žaidimo ir pasirinkti komandą.
- Žaidimas prasidės, kai bus suformuotos dvi komandos.
  
2.Pranešimai:

  - Žaidėjai gauna informacinius pranešimus apie svarbius įvykius žaidimo metu.
  
  - Žaidėjui informuojama, kai jis pataiko į priešininko laivą.
  
  - Žaidėjui pranešama, jei nepavyko pataikyti į priešininko laivą.
  
  - Žaidėjai mato savo laivų surinktus taškus.
  
3.Laivų išdėstymas:

  - Žaidėjai gali patys išsidėlioti savo laivus ant žemėlapio.
  
  - Yra galimybė atsitiktinai paskirstyti laivus automatiškai.
  
  
4.Papildomų laivų pridėjimas:

 - Yra nustatytas maksimalus skaičius papildomų laivų, kuriuos galima pridėti.
 
  - Žaidėjai gali pridėti papildomų laivų, jei neviršijama nustatyta riba.
  
5.Pasiruošimas žaidimui:

  - Žaidėjai gali paspausti "paruoštas" mygtuką, kai jie pasirengę pradėti žaidimą.
  
  - Žaidimas prasideda tik tuomet, kai abi komandos yra pasiruošusios.
  
6.Žaidimo eiga:

  - Žaidėjai paeiliui atlieka ėjimus.
  
  - Kiekvienas ėjimas apima šūvį į priešininko laivą.
  
  - Žaidėjas gali keisti žaidimo tematiką (diena/naktis), kas daro įtaką šūvio jėgos skaičiavimui.
  
  - Žaidėjas pasirenka, kuris laivas šaudys, ir bombos tipą, po to apskaičiuojamos šūvio koordinatės pagal pasirinkto laivo dydį ir bombos tipą.
  
  - Yra galimybė perkrauti žaidimą ir pradėti iš naujo.
  
  - Žaidėjui suteikiama 20 ėjimų, per kuriuos jis turi sunaikinti visus priešininko laivus; jei ėjimai baigiasi anksčiau, žaidėjas pralaimi.
  
  - Kiekvienas laivas turi ribotą šūvių kiekį; kai laivui baigiasi šūviai, jis pašalinamas iš žaidimo, o jei visi laivai iššaudo visus šūvius, žaidimas baigiasi.
  
  -Pataikius į visas laivo vietas, jo gyvybės nukrenta iki nulio.
  

### Nefunkciniai reikalavimai
