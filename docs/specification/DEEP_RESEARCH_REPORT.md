# Dyp forskningsrapport: En presis, offline C#-CAD i Fusion 360-klassen

## Hovedkonklusjon og anbefalt teknisk retning

Målet er gjennomførbart, men det er viktig å skille mellom to svært forskjellige problemer:

1. å bygge en **presis parametrisk CAD-applikasjon** med skisser, constraints, features, assemblies, B-Rep-geometri, produksjonskontroll og GPU-rendering, og  
2. å kunne lese og skrive et stort antall **proprietære CAD-formater** fra Autodesk, Dassault, Siemens, PTC og andre.

Det første kan bygges med en betydelig mengde åpen kildekode. Det andre kan **ikke realistisk løses med bare åpen kildekode** dersom støtten skal være produksjonsklar og dekke moderne `.sldprt`, `.CATPart`, `.ipt`, `.iam`, `.prt`, `.x_t`, `.sat`, `.jt`, osv. For disse bør programmet ha en modulær translator-arkitektur hvor en kommersiell SDK som HOOPS Exchange eller ODA kan installeres som et valgfritt backend-lag. HOOPS Exchange dokumenterer per september 2026 støtte for blant annet Inventor 2027, CATIA V5-6R2026, Creo 13.4, NX2512, SolidWorks 2026, ACIS, Parasolid, JT, DWG/DXF, STEP, IGES, FBX, 3MF, OBJ og STL. citeturn20view0turn20view1

Autodesk selv lister praktisk talt hele formatlisten du etterspør som formater Fusion kan arbeide med, inkludert `.wire`, `.f3d`, `.f3z`, `.iam`, `.ipt`, `.CATPart`, `.CATProduct`, Creo/NX `.prt`, `.asm`, `.3dm`, `.skp`, `.sldprt`, `.sldasm`, IGES, JT, Parasolid, SAT/SAB, STEP, 3MF, FBX, OBJ, STL, SVG, USDZ, DWG og DXF. Autodesk påpeker samtidig at oppførselen varierer med filversjon, kildeprogram, kompleksitet, referanser og assembly-struktur. Det er en viktig advarsel: «supports extension X» er ikke det samme som perfekt semantisk round-trip. citeturn4view3turn5view0

**Min anbefalte arkitektur er derfor:**

```text
Windows EXE
│
├── C# / .NET 10
│   ├── WPF GUI
│   ├── Command system
│   ├── Parametric document model
│   ├── Sketch system
│   ├── Constraint abstraction
│   ├── Feature/history engine
│   ├── Assembly model
│   ├── Undo/Redo + transactions
│   └── File-format plug-in manager
│
├── Rendering
│   ├── Direct3D 11/12 abstraction
│   ├── Vortice.Windows
│   ├── NVIDIA/high-performance GPU selection
│   ├── GPU mesh cache
│   ├── picking/highlighting
│   └── optional PBR renderer
│
├── Native CAD Core
│   └── C ABI wrapper
│       └── Open CASCADE Technology
│           ├── B-Rep
│           ├── NURBS
│           ├── curves/surfaces
│           ├── booleans
│           ├── fillets/chamfers
│           ├── intersections
│           ├── topology
│           ├── meshing
│           └── shape healing
│
├── Open exchange providers
│   ├── OCCT STEP / IGES / STL / OBJ
│   ├── lib3mf
│   ├── openNURBS
│   └── CADability / DXF helpers
│
└── Optional commercial translators
    ├── HOOPS Exchange
    ├── ODA Drawings.NET
    ├── ODA JT / STEP / InvInterop
    └── future vendor-specific bridges
```

Dette gjør **C# til programmets egentlige språk og arkitektur**, samtidig som selve geometrikjernen ligger i C++, hvor de modne CAD-kjernene allerede finnes. Å forsøke å skrive en fullverdig B-Rep/NURBS-kjerne fra bunnen av i C# ville etter min vurdering være den største tekniske feilen prosjektet kunne gjøre. Open CASCADE Technology, OCCT, er spesifikt utviklet for CAD/CAM/CAE og tilbyr solid- og overflatemodellering, datautveksling og visualisering. OCCT er hovedsakelig C++ og distribueres under LGPL 2.1 med et særskilt unntak, samt kommersielle lisensmuligheter. citeturn14view3

For selve Windows-applikasjonen ville jeg per september 2026 valgt **.NET 10 LTS**. Microsoft oppgir at .NET 10 er aktiv LTS og støttes til november 2028. WPF er fortsatt aktivt utviklet på moderne .NET og er en Windows-spesifikk, åpen GUI-teknologi, noe som passer godt når sluttproduktet uttrykkelig skal være en Windows EXE og ikke trenger å være kryssplattform i første omgang. citeturn19search0turn19search1turn19search5turn19search8

Den overordnede regelen for agenten bør være:

> **C# eier applikasjonen. En moden CAD-kjerne eier eksakt geometri. GPU-en eier visualiseringen. Filformat-backends eier oversettelsen. Ingen av disse lagene skal blandes sammen.**

## Hva som faktisk må etterlignes fra Fusion

Fusion er mer enn en «3D-modellerer». Autodesk beskriver Fusion som en integrert plattform som kombinerer CAD, CAM, CAE og elektronikk/PCB, og Fusion API-et er laget for å utvide og automatisere produktet. Programmet ditt bør derfor ikke forsøke å implementere hele Fusion-produktporteføljen samtidig; CAD-kjernen bør bygges først, mens CAM, simulation og elektronikk blir egne senere moduler. citeturn1search13turn17search11

### Parametriske skisser

Dette er en av de viktigste delene av hele systemet.

Fusion bruker 2D- og 3D-skisser som grunnlag for senere 3D-geometri. Autodesk beskriver sketch-verktøyene som verktøy for å opprette, modifisere og constrain-e 2D/3D-geometri som igjen driver den tredimensjonale modellen. citeturn1search1turn7search2

Programmet bør derfor minst ha:

| Skissegeometri | Constraint-/dimensjonsstøtte |
|---|---|
| Punkt | Coincident |
| Linje | Horizontal / Vertical |
| Polyline | Parallel |
| Sirkel | Perpendicular |
| Bue | Tangent |
| Ellipse | Concentric |
| Slot | Equal |
| Rectangle | Midpoint |
| Polygon | Collinear |
| Spline / B-spline | Symmetry |
| Construction geometry | Fix / Unfix |
| Projection/reference geometry | Curvature der relevant |
| 3D-skisse | Driving / driven dimensions |

Fusion har et omfattende system av geometriske constraints som styrer relativ posisjon mellom skisseobjekter; Autodesk dokumenterer blant annet coincident, tangent, equal, parallel, perpendicular og horizontal/vertical blant de sentrale constraint-typene. citeturn7search2turn7search6

Det må i tillegg være:

- lineær dimensjon,
- horisontal/vertikal dimensjon,
- radius,
- diameter,
- vinkel,
- arc length,
- dimensjon mellom geometri,
- driven/reference dimension,
- expressions,
- globale parametere,
- brukerparametere,
- enheter i expressions,
- constraint diagnostics,
- degrees-of-freedom-indikasjon,
- automatisk constraint-gjenkjenning,
- «fully constrained»-status.

Fusion lar brukerparametere få navn, expressions, enhet og kommentar, og verdier kan beregnes fra uttrykk. Programmet bør kopiere **konseptet**, ikke Autodesk-koden eller identiske UI-elementer. citeturn7search16turn7search0

Eksempel:

```text
plateWidth    = 120 mm
plateHeight   = 80 mm
plateThickness = 6 mm
holeDiameter  = plateThickness * 1.5
edgeOffset    = max(10 mm, holeDiameter * 2)
```

Dette bør være en sentral funksjon i dokumentmodellen, ikke et tillegg som bygges senere.

### Parametrisk solidmodellering

Minimumssettet bør omfatte:

**Create**

`Extrude`, `Revolve`, `Sweep`, `Loft`, `Hole`, `Thread`, `Rib`, `Web`, primitive box/cylinder/sphere/cone/torus.

**Modify**

`Fillet`, `Chamfer`, `Shell`, `Draft`, `Split Body`, `Split Face`, `Combine/Boolean`, `Offset Face`, `Move Face`, `Replace Face`, `Delete Face`, `Scale`.

**Pattern**

Rectangular pattern, circular pattern, pattern along path og mirror.

**Construction**

Offset planes, angled planes, tangent planes, midplanes, axes, points og lokale coordinate systems.

Autodesks egne opplæringsmaterialer for Fusion dekker blant annet extrude, revolve, fillet, chamfer, hole/thread, ribs/webs og draft. citeturn7search18turn7search32turn7search22

Hver operasjon skal bli et eget `Feature`-objekt:

```csharp
Feature
├── FeatureId
├── FeatureType
├── Name
├── InputReferences
├── Parameters
├── Suppressed
├── ResultBodies
├── DependencyIds
├── ValidationState
└── ErrorState
```

Dermed kan tidslinjen fungere omtrent som Fusion:

```text
Sketch1 → Extrude1 → Fillet1 → Hole1 → Pattern1 → Shell1
```

Brukeren skal kunne gå tilbake til `Sketch1`, endre `width = 60 mm` til `80 mm`, og hele avhengighetsgrafen skal regenereres.

### Surface- og NURBS-modellering

For et program som skal kunne konkurrere seriøst med Fusion må solidmodellering og overflatemodellering bruke samme geometriske fundament.

Det bør etter hvert støttes:

`Extrude Surface`, `Revolve Surface`, `Sweep Surface`, `Loft Surface`, `Patch/Fill`, `Offset Surface`, `Extend`, `Trim`, `Untrim`, `Split`, `Stitch/Sew`, `Unstitch`, `Thicken`, `Ruled Surface`, `Replace Face`, surface intersection og continuity-kontroller.

OCCT er spesielt egnet her fordi B-Rep-modellen holder **eksakt analytisk/NURBS-geometri og topologi**, mens en separat triangulering kan genereres for visning. OCCT-dokumentasjonen skiller eksplisitt mellom eksakt geometrirepresentasjon og tessellert mesh. citeturn10search3

Det skillet er ekstremt viktig:

```text
IKKE:

Triangle mesh == den egentlige CAD-modellen


GJØR:

Exact B-Rep
    ↓
Tessellation cache
    ↓
GPU rendering
```

Brukeren kan dermed ha en perfekt cylinder/NURBS-overflate i CAD-kjernen, mens skjermkortet bare ser millioner av triangler.

### Sheet metal

Fusion har egne sheet-metal-regler som blant annet kontrollerer tykkelse, bend radius og andre bøyeparametere, og arbeidsflyten inkluderer flanges/bends, unfolding og flat pattern. citeturn1search4

Et produksjonsrettet sheet-metal-system bør derfor ha:

- Sheet Metal Rule
- material thickness
- inside bend radius
- K-factor / bend allowance
- flange
- contour flange
- bend
- hem
- jog
- corner relief
- bend relief
- rip
- unfold
- refold
- flat pattern
- DXF-export av flat pattern

Dette bør være en separat feature-familie fordi sheet-metal-topologi har andre regler enn vanlig B-Rep-modellering.

### Assemblies og komponenter

Dokumentmodellen bør støtte:

```text
Document
└── Root Component
    ├── Component A
    │   ├── Body
    │   └── Sketch
    ├── Component B
    └── Subassembly C
        ├── Component C1
        └── Component C2
```

Deretter bygges assembly constraints/joints:

- Rigid
- Revolute
- Slider
- Cylindrical
- Pin-slot
- Planar
- Ball
- joint limits
- joint offsets
- motion links
- grounded components
- component instances
- interference detection
- sectioning
- exploded arrangements senere

En assembly-instans skal **ikke duplisere geometrien**. Den skal referere til samme definisjon med sin egen transformasjon.

### Inspection og presisjonsverktøy

Dette området bør få høyere prioritet enn fancy rendering.

Fusion sitt `Measure`-verktøy kan rapportere blant annet avstand, vinkel, areal og posisjon samt minimumsavstand mellom valgte objekter. `Interference` analyserer overlapp mellom solids/components. citeturn7search1

Programmet bør derfor ha et komplett Inspect-workspace med:

- point coordinates,
- edge length,
- curve radius,
- minimum radius,
- angle,
- face area,
- surface type,
- center of mass,
- volume,
- bounding box,
- center-to-center,
- point-to-face,
- face-to-face minimum distance,
- wall thickness,
- interference,
- section analysis,
- draft-angle analysis,
- curvature analysis,
- surface continuity,
- open-edge detection,
- non-manifold detection,
- self-intersection detection,
- body validity report,
- imported tolerance report.

**Visningspresisjon og geometrisk presisjon skal være to forskjellige systemer.** Autodesk lar brukeren endre antall viste desimaler gjennom preference-systemet, mens faktiske geometriske toleranser håndteres på et annet nivå. citeturn4view4

Det betyr:

```text
Displayed:
25.40 mm

Internal:
25.400000000000... mm

Modeling tolerance:
separat verdi

Export tolerance:
separat verdi

Mesh chord tolerance:
separat verdi
```

### Form, mesh og visual modeling

Fusion inkluderer også T-spline/Form-relaterte arbeidsflyter og kobling mellom form-geometri og parametrisk modellering. Autodesk har blant annet dokumentert associative workflows mellom Form og parametriske bodies. citeturn17search11

Dette bør ikke være første implementasjon. Først bør mesh-modulen dekke:

- import mesh,
- inspect,
- select vertices/edges/faces,
- transform,
- weld,
- remove duplicate vertices,
- recalculate normals,
- invert normals,
- remove degenerate triangles,
- close simple holes,
- reduce/decimate,
- remesh,
- section mesh,
- mesh-to-BRep for begrensede modeller,
- BRep-to-mesh med kontrollerbar chord tolerance.

Full T-spline/SubD-modellering kan komme etter at B-Rep-systemet er stabilt.

## Presisjonsarkitekturen som bør bygges først

Dette er etter min vurdering den viktigste delen av hele prosjektet.

Et CAD-program kan se fantastisk ut og fortsatt produsere ødelagte STEP-filer. Derfor må geometrisk korrekthet være en systemegenskap.

### Velg én eksakt kernel som sannhetskilde

OCCT bør være førstevalget for den åpne kjernen.

OCCT tilbyr CAD-orientert surface/solid modeling, Boolean-operasjoner, data exchange og visualisering. De offisielle eksemplene bruker `BRepAlgoAPI` for Boolean common/cut/fuse. citeturn14view3turn10search11

C#-applikasjonen bør **ikke** lagre sin egen alternative B-Rep parallelt.

Bruk:

```text
C# feature definition
        ↓
Native Kernel request
        ↓
OCCT TopoDS_Shape
        ↓
exact model result
        ↓
derived visualization mesh
```

### Lag et ekstremt smalt native API

Ikke expose tusenvis av OCCT-klasser direkte til C#.

Lag noe tilsvarende:

```cpp
CadResult cad_create_box(...);
CadResult cad_extrude(...);
CadResult cad_revolve(...);
CadResult cad_boolean(...);
CadResult cad_fillet(...);
CadResult cad_chamfer(...);
CadResult cad_validate_shape(...);
CadResult cad_tessellate(...);
CadResult cad_import_step(...);
CadResult cad_export_step(...);
```

Og returner opaque handles:

```csharp
readonly struct ShapeHandle
{
    public readonly ulong Value;
}
```

Dette gjør at OCCT kan oppgraderes uten at hele C#-kodebasen blir bundet til intern C++-ABI.

### Skill alle toleranser fra hverandre

Ikke ha:

```csharp
double Tolerance = 0.001;
```

som brukes overalt.

Ha i stedet:

```csharp
PrecisionSettings
{
    ModelingLinearTolerance
    ModelingAngularTolerance

    ConstraintLinearTolerance
    ConstraintAngularTolerance

    CoincidenceTolerance

    SewingTolerance
    ImportHealingTolerance
    MaximumHealingTolerance

    IntersectionTolerance

    TessellationChordTolerance
    TessellationAngularTolerance

    SelectionTolerance
    SnapTolerance

    ExportTolerance
}
```

Autodesk advarer spesifikt om «loose tolerances» på importert geometri: avvik mellom CAD-systemer kan gi edges/vertices med økte toleranser, og slike avvik kan senere få modelling-operasjoner til å feile. Autodesk beskriver også Stitch som en operasjon hvor en toleransesone kan brukes uten nødvendigvis å endre den underliggende geometrien. citeturn4view5

Det bør derfor finnes en dialog som viser:

```text
Geometry Health

Maximum vertex tolerance:     ...
Maximum edge tolerance:       ...
Open boundaries:              ...
Invalid wires:                ...
Degenerate edges:             ...
Self intersections:           ...
Non-manifold regions:         ...
Tiny edges:                   ...
Tiny faces:                   ...
Sewing operations performed:  ...
Healing operations performed: ...
```

### Valider etter alle viktige topologiske operasjoner

OCCT har `BRepCheck_Analyzer` for kontroll av samlet shape-validity og peker selv til ShapeAnalysis, ShapeUpgrade og ShapeFix for diagnostikk og korrigering. citeturn11search22

Pipeline:

```text
Feature calculation
      ↓
Kernel result
      ↓
BRepCheck_Analyzer
      ↓
Valid?
 ┌────┴─────┐
 yes        no
 ↓           ↓
commit    diagnostic
          ↓
   conservative healing
          ↓
      validate again
```

Shape healing må **ikke skjules** for brukeren. OCCTs shape-healing-system kan eksempelvis lukke gap eller øke toleranser; derfor bør alle slike endringer registreres. citeturn10search35

Eksempel:

```text
Import Report
-------------
2 wires repaired
3 vertices merged
1 gap sewn
maximum resulting tolerance: ...
1 face rejected
```

### Løs «topological naming problem» fra begynnelsen

Dette er et av de vanskeligste problemene i parametrisk CAD.

Tenk:

```text
Sketch
  ↓
Cube
  ↓
Fillet edge #4
```

Så endrer brukeren skissen.

Etter regenerering er det kanskje ikke lenger en «edge #4».

OCCTs OCAF-dokumentasjon påpeker nettopp at topologien kan forandre seg gjennom modeling-operasjoner; en Boolean kan for eksempel splitte en edge. citeturn10search7

**Aldri lagre:**

```text
"Fillet EdgeIndex=4"
```

som eneste referanse.

Lagre i stedet en semantisk `TopoReference`:

```csharp
TopoReference
{
    SourceFeatureId
    GeneratedByFeatureId
    GeometryType
    CreationRelation
    ApproximateCentroid
    DirectionOrNormal
    RadiusIfRelevant
    AdjacentFaceSignatures
    PersistentKernelIdIfAvailable
}
```

Ved recompute:

```text
Persistent ID
     ↓ fails
Feature generation relation
     ↓ fails
Geometric signature match
     ↓ fails
User repair requested
```

Dette må designes før feature-tidslinjen blir stor.

### Constraint-solveren må være et eget subsystem

FreeCAD er svært relevant som forskningsgrunnlag. FreeCAD er en parametrisk, historie-basert CAD-applikasjon med constraint-baserte 2D-skisser og bygger selv på Open CASCADE. citeturn4view2

FreeCAD har også kildekoden til sin PlaneGCS constraint solver tilgjengelig, som gjør den verdifull å studere for struktur, constraint mapping, numerical solution og diagnoser. citeturn13search5

Men ikke kopier kildekode tilfeldig. FreeCAD distribueres under LGPL-betingelser, så gjenbruk må planlegges lisensmessig. citeturn18search26

En egen C#-solver bør deles opp slik:

```text
SketchModel
   ↓
ConstraintGraph
   ↓
EquationGenerator
   ↓
NumericalSolver
   ↓
Residual evaluator
   ↓
DOF analysis
   ↓
Conflict detector
```

Agenten bør implementere constraint-familiene trinnvis:

```text
Coincident
→ Horizontal / Vertical
→ Distance
→ Radius
→ Parallel
→ Perpendicular
→ Equal
→ Tangent
→ Concentric
→ Midpoint
→ Symmetry
→ advanced spline constraints
```

CADability er også særlig interessant fordi prosjektet er skrevet for .NET/C#, inneholder 3D-CAD-geometri, STEP/STL/DXF-utveksling og parametric-relaterte funksjoner. CADability oppgir MIT-lisens og bruker blant annet ACadSharp for DWG/DXF-relatert funksjonalitet. citeturn14view2turn18search24

Det er sannsynligvis det **mest relevante åpne C#-prosjektet å studere direkte**.

## Filformatstrategien og hva som faktisk er mulig

Den store feilen ville være å lage én gigantisk `ImportFile()`-klasse.

Bruk dette:

```csharp
public interface ICadFormatProvider
{
    IReadOnlyCollection<string> Extensions { get; }

    FormatCapabilities Capabilities { get; }

    Task<ImportResult> ImportAsync(
        string path,
        ImportOptions options,
        CancellationToken cancellationToken);

    Task<ExportResult> ExportAsync(
        CadDocument document,
        string path,
        ExportOptions options,
        CancellationToken cancellationToken);
}
```

Deretter:

```text
OCCTProvider
Lib3MFProvider
OpenNurbsProvider
DxfProvider
OdaProvider
HoopsProvider
OpenUsdProvider
SvgProvider
FusionArchiveProvider
SketchUpProvider
```

### Anbefalt formatmatrise

| Format | Strategi | Prioritet / realisme |
|---|---|---|
| `.step`, `.stp`, `.ste` | OCCT STEP reader/writer; `.ste` routet som STEP | **Kjernefunksjon** |
| `.iges`, `.igs` | OCCT | **Kjernefunksjon** |
| `.stl` | OCCT | **Kjernefunksjon** |
| `.obj` | OCCT | **Kjernefunksjon** |
| `.3mf` | lib3mf | **Kjernefunksjon** |
| `.dxf` | CADability/ACadSharp eller egen provider | **Høy prioritet** |
| `.dwg` | ODA Drawings.NET eller HOOPS | **Kommersiell backend anbefalt** |
| `.3dm` | openNURBS eller HOOPS | **God løsning finnes** |
| `.sat`, `.sab` | HOOPS/kommersiell ACIS-translator | **Kommersiell** |
| `.x_t`, `.x_b` | HOOPS/Parasolid translator | **Kommersiell** |
| `.jt` | HOOPS eller ODA JT SDK | **Kommersiell** |
| `.ipt`, `.iam` | HOOPS/ODA InvInterop etter evaluering | **Kommersiell** |
| `.sldprt`, `.sldasm` | HOOPS Exchange | **Kommersiell** |
| `.CATPart`, `.CATProduct` | HOOPS Exchange | **Kommersiell** |
| Creo `.prt`, `.asm` | HOOPS Exchange | **Kommersiell** |
| NX `.prt` | HOOPS Exchange | **Kommersiell** |
| `.fbx` | HOOPS eller separat FBX-provider | **Mesh/scene-format** |
| `.svg` | egen SVG→Sketch-provider | **Realistisk å implementere** |
| `.usdz` | OpenUSD native backend | **Realistisk, men eget subsystem** |
| `.skp` | dedikert SketchUp-provider / lisensiert SDK | **Egen adapter nødvendig** |
| `.wire` | vendor/licensed translator | **Vanskelig proprietært format** |
| `.f3d`, `.f3z` | Autodesk-spesifikk bridge når lovlig/dokumentert standalone-metode finnes | **Største blocker** |

OCCTs moderne Data Exchange Wrapper dokumenterer nativ lesing/skriving av STEP, IGES, OBJ, STL og glTF, med egne egenskaper for B-Rep kontra mesh og thread safety. XDE-laget kan i tillegg bevare assembly-struktur, navn, farger og andre attributter der kildeformatet tilbyr dem. citeturn16search4turn16search6

STEP bør bli programmets viktigste nøytrale **presisjonsformat**. OCCT støtter STEP AP203/AP214/AP242, og STEP-translatorene oversetter til/fra ekte CAD-shapes i stedet for bare displaytriangler. citeturn16search0turn10search37

For `.3mf` finnes 3MF Consortiums `lib3mf`, som er en C++-implementasjon av 3MF-formatet med lesing, skriving, konvertering og validering og API-er beregnet på integrasjon fra flere programmeringsspråk. citeturn15search12

For Rhino `.3dm` finnes McNeels åpne `openNURBS`-toolkit, eksplisitt laget slik at eksterne programmer kan lese og skrive 3DM uten Rhino og med støtte for NURBS-relaterte data. citeturn14search1

For USDZ bør man bruke OpenUSD som referanseimplementasjon/backend. Pixar beskriver USD som et system for hierarkisk scene interchange, og USDZ-spesifikasjonen definerer pakken som en ukomprimert ZIP-basert pakke med USD- og relaterte assets. citeturn14search6turn14search8turn14search10

### Proprietære CAD-formater

Dette er stedet hvor det er verdt å betale for teknologi.

HOOPS Exchange dokumenterer akkurat den kombinasjonen prosjektet trenger:

- ACIS `.sat/.sab`
- DWG/DXF
- Inventor `.ipt/.iam`
- CATIA `.CATPart/.CATProduct`
- Creo `.prt/.asm`
- FBX
- IGES
- JT
- NX `.prt`
- Parasolid `.x_t/.x_b`
- Rhino `.3dm`
- SolidWorks `.sldprt/.sldasm`
- STEP
- 3MF
- STL
- OBJ. citeturn20view0turn20view1

HOOPS oppgir dessuten at importerlaget kan levere assembly-tree, transforms, instances, B-Rep, PMI, visualiseringsdata, persistent IDs, face names, coordinate systems og andre attributter, avhengig av format. Det er nettopp denne typen semantisk informasjon som gjør en profesjonell translator svært mye mer verdifull enn en enkel «convert everything to triangles»-løsning. citeturn20view0

Det finnes også en C#-API-overlay til HOOPS Exchange, slik at det kan integreres i den foreslåtte C#-arkitekturen selv om selve oversetteren er native. citeturn9search2turn9search13

ODA er en alternativ eller komplementær løsning. Den aktuelle ODA-dokumentasjonen viser egne SDK-er og C#-API-er for blant annet Drawings/DWG, STEP, Visualize, Kernel samt separate JT-, IGES-, SWInterop- og InvInterop-komponenter. citeturn14view1

### Problemet med F3D og F3Z

Dette må sies helt tydelig.

Autodesk dokumenterer `.f3d` og `.f3z` som Fusion-native arkivformater, og F3Z brukes blant annet for design med relaterte/refererte Fusion-data. citeturn15search3turn20view3

**I den offentlige dokumentasjonen og SDK-ene jeg undersøkte fant jeg ingen dokumentert, selvstendig open-source eller redistribuerbar standalone F3D/F3Z-parser som kan legges direkte inn i en ny CAD-applikasjon og gi full Fusion feature-history.**

Derfor skal agenten **ikke** begynne å reverse-engineere F3D/F3Z som fundament for produktet.

Lag i stedet:

```text
FusionArchiveProvider
├── Detect
├── Metadata inspection
├── ITranslatorBackend
└── status:
    ├── NativeSupported
    ├── RequiresLicensedProvider
    ├── RequiresExternalBridge
    └── UnsupportedVersion
```

Full `.f3d/.f3z`-støtte bør først annonseres når prosjektet faktisk har en lovlig, robust translator. Et Fusion-installert bridge-system kan eventuelt vurderes senere, men det bør ikke være en skjult forutsetning for den normale CAD-kjernen.

Autodesk påpeker også at eksport til nøytrale formater som STEP/IGES ikke nødvendigvis beholder den opprinnelige parametriske feature-historikken; resultatet kan være geometrisk korrekt men uten den opprinnelige designhistorikken. citeturn1search15

Derfor må programmets eget native format være et **eget åpent dokumentformat**.

Jeg ville eksempelvis kalt det:

```text
.cadx
```

med struktur:

```text
MyPart.cadx
│
├── manifest.json
├── document.json
├── parameters.json
├── features.json
├── assemblies.json
├── materials.json
├── references.json
├── geometry/
│   ├── body_001.brep
│   └── body_002.brep
└── preview/
    └── thumbnail.webp
```

Komprimer det til én fil, men behold en eksplisitt, versjonert intern struktur.

## Åpen kildekode agenten bør studere

### Open CASCADE Technology

**Bruk til:** geometrikjerne.

OCCT er den klart viktigste kandidaten. Prosjektet er laget for CAD/CAM/CAE og inneholder surface/solid modeling, CAD-datautveksling og visualisering. citeturn14view3

Studer spesielt:

```text
TopoDS
BRep
BRepBuilderAPI
BRepAlgoAPI
BRepFilletAPI
BRepOffsetAPI
Geom
Geom2d
gp
GCE / GC
ShapeFix
ShapeAnalysis
BRepCheck
BRepMesh
STEPCAFControl
IGESCAFControl
XCAF
OCAF
```

### FreeCAD

**Bruk til:** arkitekturforskning.

FreeCAD er spesielt verdifull fordi det demonstrerer hvordan en komplett open-source parametrisk desktop-CAD kan organiseres rundt Open CASCADE, constrained sketches, feature history og GUI. FreeCAD bruker OpenCASCADE som kernel, Coin3D for visualisering, Qt for GUI og har omfattende scripting/API-funksjonalitet. citeturn4view2

Ikke kopier FreeCAD-GUI-en. Studer:

- document recompute,
- feature dependency,
- Sketcher,
- PlaneGCS,
- Part/PartDesign,
- serialization,
- property system,
- transactions,
- topology references.

### CADability

**Bruk til:** C#-referanse.

Dette er kanskje det mest nyttige kodeprosjektet for akkurat ditt krav om C#.

CADability beskriver seg som et rent .NET CAD-bibliotek for modellering og analyse av tredimensjonale data, har UI-støtte, data exchange for blant annet STEP/STL/DXF og parametriske funksjoner. Prosjektet er MIT-lisensiert. citeturn14view2turn18search24

Studer spesielt:

- geometriobjektmodell,
- C# API-design,
- selection,
- actions/commands,
- project/document layout,
- STEP parsing,
- DXF/DWG-integration,
- parameter-modellen.

Det er et godt sted å hente **C#-designmønstre**, mens OCCT bør håndtere den mest krevende eksakte B-Rep-modelleringen.

### FreeCAD PlaneGCS

**Bruk til:** constraint-solver-forskning.

PlaneGCS-koden finnes i FreeCADs offisielle repository og viser en reell geometric constraint solver fra et etablert parametrisk CAD-system. citeturn13search5

Agenten bør bruke den som faglig referanse til:

```text
constraints → equations → solver → residuals → DOF
```

ikke som grunn til å lime enorme C++-kodeblokker direkte inn i C#.

### lib3mf

**Bruk til:** 3MF.

`lib3mf` er 3MF Consortiums referanseorienterte implementasjon for 3MF-lesing/skriving og validation/conversion. citeturn15search12

Lag en liten native wrapper rundt den og konverter til programmets interne `MeshDocument`.

### openNURBS

**Bruk til:** `.3dm`.

McNeels openNURBS er eksplisitt laget for tredjepartslesing og -skriving av Rhino 3DM og inkluderer NURBS-evalueringsfunksjonalitet. citeturn14search1

Importer deretter NURBS curves/surfaces og topology inn i OCCT-representasjonen der det er mulig.

### Vortice.Windows

**Bruk til:** DirectX fra C#.

Vortice.Windows tilbyr .NET-bindings til Windows graphics/API-teknologi inkludert DXGI, Direct3D 11 og Direct3D 12. citeturn12search0

Det gir en renere løsning enn å prøve å la WPF selv tegne store CAD-modeller.

### NVIDIA og GPU-valg

Programmet bør ikke basere NVIDIA-støtten på gamle vendor-spesifikke «Optimus hacks».

Microsoft tilbyr DXGI/DXCore-mekanismer for å sortere/enumerere adaptere etter GPU-preference, blant annet `HighPerformance`. Dette gjør det mulig å velge diskret NVIDIA-GPU der systemet rapporterer den som high-performance-adapter, samtidig som brukeren kan overstyre valget. citeturn12search1turn12search8

Innstillingen bør se omtrent slik ut:

```text
Graphics Device
○ Automatic
● NVIDIA GeForce RTX ...
○ Integrated GPU
○ Microsoft WARP

GPU preference
● High performance
○ Power saving

Renderer
● Direct3D 11
○ Direct3D 12 Experimental

Anti-aliasing
4x MSAA

Edge rendering
● Enabled

Ambient occlusion
● Enabled

PBR materials
● Enabled

Dynamic tessellation
● Enabled
```

## Steg-for-steg planen agenten bør følge

Dette er rekkefølgen jeg ville gitt en autonom kodeagent. **Ikke hopp direkte til filformatene eller fancy GUI.**

### Fundament og repository

**Steg 1.** Opprett GitHub-repository med `main`, feature branches, pull-request-krav og CI.

**Steg 2.** Opprett solution:

```text
CadApp.sln

/src
  Cad.App
  Cad.UI
  Cad.Commands
  Cad.Document
  Cad.Features
  Cad.Sketch
  Cad.Assembly
  Cad.Geometry
  Cad.Kernel.Interop
  Cad.Rendering
  Cad.IO
  Cad.Persistence
  Cad.Settings
  Cad.Diagnostics

/native
  CadKernel
  ThirdParty

/tests
  Cad.UnitTests
  Cad.KernelTests
  Cad.FileFormatTests
  Cad.RenderingTests
  Cad.IntegrationTests
  Cad.RegressionTests

/assets
/docs
/tools
```

**Steg 3.** Target `.NET 10` for Windows. Det er den aktuelle LTS-versjonen med støtte til november 2028. citeturn19search0turn19search4

**Steg 4.** Bruk WPF kun for vindussystem, panels, menus, dialogs og controls. Ikke bruk WPF 3D som den endelige CAD-rendereren.

**Steg 5.** Aktiver nullable reference types, analyzers, warnings-as-errors i kjernelagene og deterministic builds.

**Steg 6.** Opprett et sentralt `Result<T>` / error-system. Ingen geometrifeil skal representeres ved `null`.

**Steg 7.** Definer stabile GUID-er for documents, components, features, sketches, constraints, bodies og parameters.

**Steg 8.** Implementer structured logging fra dag én.

**Steg 9.** Lag crash-dump- og recovery-infrastruktur før stor funksjonalitet.

**Steg 10.** Lag en liten diagnostics-app som kan teste den native kjernen uten full GUI.

### Geometrikjernen

**Steg 11.** Legg inn OCCT som pinned native dependency. OCCT er bygget for CAD/CAM/CAE og har den B-Rep-/surface-funksjonaliteten prosjektet trenger. citeturn14view3

**Steg 12.** Ikke la C# referere direkte til OCCT-klasser.

**Steg 13.** Opprett `CadKernel.dll` i C++ med C ABI.

**Steg 14.** Implementer safe opaque handles.

**Steg 15.** Implementer memory ownership:

```text
Create → Handle
Clone → Handle
Release → void
```

**Steg 16.** Implementer native error-stack med kode, melding og kernel context.

**Steg 17.** Implementer primitives: point, vector, axis, plane og transform.

**Steg 18.** Implementer line, circle, arc, ellipse og B-spline.

**Steg 19.** Implementer plane, cylinder, cone, sphere og NURBS surface.

**Steg 20.** Implementer topology handles for vertex, edge, wire, face, shell, solid og compound.

**Steg 21.** Implementer box, cylinder, sphere, cone og torus.

**Steg 22.** Implementer extrude/prism.

**Steg 23.** Implementer revolve.

**Steg 24.** Implementer Boolean `Fuse`, `Cut`, `Common`. OCCT tilbyr BRepAlgoAPI for nettopp Boolean-operasjonene. citeturn10search11

**Steg 25.** Implementer fillet.

**Steg 26.** Implementer chamfer.

**Steg 27.** Implementer shell/thickness.

**Steg 28.** Implementer loft og sweep.

**Steg 29.** Implementer surface trim, sew/stitch og thicken.

**Steg 30.** Kjør `BRepCheck_Analyzer` på alle produksjonskritiske resultater. citeturn11search22

### Dokumentmodellen

**Steg 31.** Lag `CadDocument`.

```text
CadDocument
├── Metadata
├── Units
├── Parameters
├── RootComponent
├── Features
├── Materials
├── Views
└── PrecisionSettings
```

**Steg 32.** Lag component/body-modellen.

**Steg 33.** Skill `BodyDefinition` fra `ComponentInstance`.

**Steg 34.** Opprett directed dependency graph.

**Steg 35.** En feature skal deklarere hvilke features/sketches/parameters den avhenger av.

**Steg 36.** Lag dirty propagation:

```text
Parameter changed
      ↓
Sketch dirty
      ↓
Extrude dirty
      ↓
Fillet dirty
      ↓
Pattern dirty
```

**Steg 37.** Recompute bare den påvirkede delen av grafen.

**Steg 38.** Lag transactional commit. En mislykket feature skal ikke korrumpere forrige gyldige document state.

**Steg 39.** Implementer undo/redo som commands/transactions, ikke som kopiering av hele dokumentet.

**Steg 40.** Implementer suppression av features.

**Steg 41.** Implementer feature reorder med dependency validation.

**Steg 42.** Implementer feature rollback marker.

### Parameter- og expression-system

**Steg 43.** Lag `Quantity` med value + dimension.

Ikke representer:

```text
10
```

Representer:

```text
10 mm
45 deg
12 mm²
100 mm³
```

**Steg 44.** Implementer enhetsfamilier:

```text
mm
cm
m
µm
inch
ft
deg
rad
```

**Steg 45.** Lag expression parser.

**Steg 46.** Lag named user parameters slik Fusion gjør konseptuelt. Autodesk lar parameters bruke navn, expressions og units. citeturn7search16turn7search0

**Steg 47.** Opprett dependency graph mellom parametere.

**Steg 48.** Detect circular expressions.

```text
A = B * 2
B = A / 2
```

skal gi tydelig feil.

**Steg 49.** Lag math functions:

```text
sin
cos
tan
asin
acos
sqrt
abs
min
max
floor
ceil
```

**Steg 50.** Skill display precision fra modelling tolerance. Autodesk gjør også display precision konfigurerbar separat. citeturn4view4

### Skisseengine

**Steg 51.** Lag `SketchPlane`.

**Steg 52.** Lag skisseentity-interface.

**Steg 53.** Implementer point.

**Steg 54.** Implementer line.

**Steg 55.** Implementer circle.

**Steg 56.** Implementer arc.

**Steg 57.** Implementer ellipse.

**Steg 58.** Implementer rectangle, polygon og slot som convenience commands over primitive entities.

**Steg 59.** Implementer construction geometry.

**Steg 60.** Implementer geometric projection/reference geometry.

**Steg 61.** Bygg ConstraintGraph.

**Steg 62.** Implementer coincident.

**Steg 63.** Implementer horizontal/vertical.

**Steg 64.** Implementer point-to-point distance.

**Steg 65.** Implementer horizontal/vertical distance.

**Steg 66.** Implementer radius/diameter.

**Steg 67.** Implementer parallel/perpendicular.

**Steg 68.** Implementer tangent.

**Steg 69.** Implementer equal.

**Steg 70.** Implementer concentric.

**Steg 71.** Implementer midpoint.

**Steg 72.** Implementer symmetry.

**Steg 73.** Implementer fix/unfix.

Dette dekker store deler av constraint-familiene Autodesk dokumenterer for Fusion. citeturn7search2turn7search6

**Steg 74.** Vis graden av frihet visuelt.

**Steg 75.** Vis unconstrained entities annerledes enn fully constrained entities.

**Steg 76.** Implementer over-constraint detection.

**Steg 77.** Returner hvilke constraints som er i konflikt.

**Steg 78.** Ikke la solver-failure krasje UI.

**Steg 79.** Lag solver tracing:

```text
Iterations: 8
Residual: ...
DOF before: 7
DOF after: 0
Conflicting constraints: none
```

### Parametriske features

**Steg 80.** Implementer `ExtrudeFeature`.

**Steg 81.** Implementer extent modes:

```text
Distance
Symmetric
Two sided
To object
Through all
```

**Steg 82.** Implementer operation:

```text
New Body
Join
Cut
Intersect
```

**Steg 83.** Implementer Revolve.

**Steg 84.** Implementer Hole.

**Steg 85.** Implementer Fillet.

**Steg 86.** Implementer Chamfer.

**Steg 87.** Implementer Draft.

**Steg 88.** Implementer Shell.

**Steg 89.** Implementer Sweep.

**Steg 90.** Implementer Loft.

**Steg 91.** Implementer Rib.

**Steg 92.** Implementer Mirror.

**Steg 93.** Implementer Rectangular Pattern.

**Steg 94.** Implementer Circular Pattern.

**Steg 95.** Implementer Split Body/Face.

**Steg 96.** Implementer Combine.

Autodesks Fusion-materiale dekker den samme generelle familien av solid-feature-operasjoner som extrude, revolve, fillet, chamfer, hole/thread, rib/web og draft. citeturn7search18turn7search22turn7search32

### Robust feature-referencing

**Steg 97.** Lag stable feature GUID.

**Steg 98.** Lag semantic topology signature.

**Steg 99.** Lag kernel history mapping for «generated from» og «modified from».

**Steg 100.** Ikke bruk edge-list-index som persistent reference.

**Steg 101.** Test edge splitting etter Boolean. OCCT dokumenterer at topologiske operasjoner kan endre/splitte topologien. citeturn10search7

**Steg 102.** Lag fallback geometric matching.

**Steg 103.** Lag «Reference lost»-UI hvor brukeren kan velge en ny edge/face.

### GPU-rendereren

**Steg 104.** Lag renderer som separat assembly.

**Steg 105.** Start med Direct3D 11 backend for første stabile produksjonsrenderer; gjør interfacet samtidig kompatibelt med en senere D3D12-backend.

**Steg 106.** Bruk Vortice.Windows til DXGI/Direct3D-interoperabilitet fra C#. citeturn12search0

**Steg 107.** Enumerer GPU-adaptere.

**Steg 108.** Bruk DXGI/DXCore high-performance-preference ved `Automatic`. Microsoft dokumenterer APIs for å enumerere/sortere adaptere etter GPU-preference. citeturn12search1turn12search8

**Steg 109.** La brukeren manuelt velge NVIDIA-adapter.

**Steg 110.** Lag device-loss recovery.

**Steg 111.** Ikke bruk eksakt CAD-geometri direkte til rendering.

**Steg 112.** Tesseller B-Rep til triangles med OCCT. OCCT tilbyr egne B-Rep-meshing-funksjoner nettopp for dette skillet. citeturn10search3

**Steg 113.** Ha flere LOD-nivåer.

```text
LOD 0: coarse
LOD 1: medium
LOD 2: fine
LOD 3: manufacturing inspection
```

**Steg 114.** Background-tesseller geometrien på worker threads.

**Steg 115.** Upload mesh asynchronously til GPU.

**Steg 116.** Cache vertex/index buffers per shape revision.

**Steg 117.** Invalider bare mesh for endrede bodies.

**Steg 118.** Implementer depth buffer.

**Steg 119.** Implementer back-face culling.

**Steg 120.** Implementer MSAA.

**Steg 121.** Implementer wire/edge overlay separat.

**Steg 122.** Implementer selection highlighting.

**Steg 123.** Implementer preselection under mouse cursor.

**Steg 124.** Implementer GPU ID-buffer eller CPU BVH for picking.

**Steg 125.** Implementer clipping/section planes.

**Steg 126.** Implementer orthographic og perspective camera.

**Steg 127.** Implementer standard views:

```text
Top
Bottom
Front
Back
Left
Right
Isometric
```

**Steg 128.** Legg PBR og avanserte materialer etter at CAD-vieweren er stabil. OCCTs egen viewer har også real-time PBR-støtte, som kan brukes som referanse eller alternativ backend. citeturn10search39

### Fusion-inspirert GUI

**Steg 129.** Lag hovedlayout:

```text
┌───────────────────────────────────────────────────────────┐
│ File  Edit    Workspace       Commands          Account?  │
├───────────────┬───────────────────────────────────────────┤
│ Toolbar/Ribbon│                                           │
├───────────────┤                                           │
│ Browser       │                  Canvas                   │
│               │                                      Cube │
│ Components    │                                           │
│ Bodies        │                                           │
│ Sketches      │                                           │
│ Origin        │                                           │
│               │                                           │
├───────────────┴───────────────────────────────────────────┤
│ Parametric Timeline                                      │
└───────────────────────────────────────────────────────────┘
```

**Steg 130.** Bruk samme **arbeidslogikk** som Fusion, men ikke kopier Autodesk-logoer, artwork eller proprietære ressurser.

**Steg 131.** Lag workspaces:

```text
Design
Sketch
Surface
Sheet Metal
Mesh
Assembly
Drawing
Inspect
Manufacture (future)
Simulation (future)
```

**Steg 132.** Lag command search.

**Steg 133.** Lag keyboard shortcut editor.

**Steg 134.** Lag radial/context menu senere.

**Steg 135.** Lag command palette:

```text
Press S → Search Commands
```

**Steg 136.** Lag dockable browser.

**Steg 137.** Lag contextual property panel.

**Steg 138.** Lag timeline nederst.

**Steg 139.** Lag multi-document tabs.

**Steg 140.** Lag navigation cube.

**Steg 141.** Lag navigation bar for orbit/pan/zoom/fit/section.

### Import/export-laget

**Steg 142.** Implementer `ICadFormatProvider`.

**Steg 143.** Lag format detection basert på både extension og header der mulig.

**Steg 144.** Importer i en separat worker-prosess for risikable/proprietære filer.

Det betyr:

```text
CadApp.exe
   ↓
CadImportWorker.exe
   ↓
translator SDK
```

Hvis en tredjepartstranslator krasjer:

```text
worker dies
≠
whole CAD application dies
```

**Steg 145.** Første provider: STEP med OCCT. OCCTs Data Exchange-system støtter STEP, IGES og flere meshformater, og kan jobbe gjennom et XDE/XCAF-dokument for metadata og assemblies. citeturn16search0turn16search4turn16search6

**Steg 146.** Implementer STEP AP242 først som foretrukket engineering-export.

**Steg 147.** Implementer IGES.

**Steg 148.** Implementer STL.

**Steg 149.** Implementer OBJ.

**Steg 150.** Integrer lib3mf.

**Steg 151.** Integrer 3DM/openNURBS.

**Steg 152.** Implementer SVG→Sketch.

**Steg 153.** Implementer DXF→Sketch/Drawing.

**Steg 154.** Evaluer ODA Drawings.NET for DWG. ODA tilbyr dokumenterte .NET/C# API-er for Drawings. citeturn14view1

**Steg 155.** Implementer `HoopsExchangeProvider` dersom prosjektet får lisens.

**Steg 156.** Map HOOPS B-Rep til OCCT bodies.

**Steg 157.** Bevar assembly transforms/names/attributes.

**Steg 158.** Bevar persistent IDs fra translatoren der de finnes. HOOPS dokumenterer tilgang til assembly-data, B-Rep, PMI og blant annet persistent IDs og face names. citeturn20view0

**Steg 159.** Implementer SAT/SAB.

**Steg 160.** Implementer Parasolid X_T/X_B.

**Steg 161.** Implementer JT.

**Steg 162.** Implementer IPT/IAM.

**Steg 163.** Implementer SolidWorks.

**Steg 164.** Implementer CATIA.

**Steg 165.** Implementer Creo.

**Steg 166.** Implementer NX.

**Steg 167.** Implementer FBX.

**Steg 168.** Implementer USDZ/OpenUSD.

**Steg 169.** Lag separat SketchUp-provider.

**Steg 170.** Hold F3D/F3Z/WIRE bak capability flags inntil en legitim translator finnes.

### Import-healing

**Steg 171.** Etter CAD-import: valider alle shapes.

**Steg 172.** Finn invalid wires.

**Steg 173.** Finn gaps.

**Steg 174.** Finn edge/vertex tolerance outliers.

**Steg 175.** Kjør bare konservativ healing automatisk.

**Steg 176.** Krev eksplisitt brukervalg for aggressiv healing.

**Steg 177.** Logg alle modifikasjoner. Autodesk fremhever at toleranseproblemer i importerte modeller kan gi senere modelleringsfeil. citeturn4view5

### Produksjonsvalidering

**Steg 178.** Lag `ManufacturingCheck`.

**Steg 179.** Krev gyldig closed solid for «production ready solid».

**Steg 180.** Kontroller manifold topology.

**Steg 181.** Kontroller self-intersections.

**Steg 182.** Kontroller zero-length/tiny edges.

**Steg 183.** Kontroller degenererte faces.

**Steg 184.** Kontroller shell orientation.

**Steg 185.** Kontroller open boundaries.

**Steg 186.** Kontroller body volume.

**Steg 187.** Kontroller geometriske toleranser.

**Steg 188.** Kontroller eksport-enheter.

**Steg 189.** Exporter STEP.

**Steg 190.** Importer samme STEP tilbake i en ny dokumentkontekst.

**Steg 191.** Sammenlign:

```text
solid count
volume
surface area
bounding box
component count
face count
critical dimensions
```

**Steg 192.** Marker eksport som bestått/avvist.

Dette er den typen round-trip-verifikasjon som bør skille programmet fra hobby-CAD.

### Assemblies

**Steg 193.** Implementer component definitions.

**Steg 194.** Implementer component instances.

**Steg 195.** Implementer assembly transform tree.

**Steg 196.** Implementer grounding.

**Steg 197.** Implementer rigid joint.

**Steg 198.** Implementer revolute joint.

**Steg 199.** Implementer slider.

**Steg 200.** Implementer cylindrical.

**Steg 201.** Implementer planar.

**Steg 202.** Implementer ball.

**Steg 203.** Implementer joint limits.

**Steg 204.** Implementer interference detection. Fusion har et eget Interference-inspection-verktøy for overlappende solids/components. citeturn7search1

### Sheet metal

**Steg 205.** Opprett SheetMetalRule.

**Steg 206.** Implementer constant-thickness validation.

**Steg 207.** Implementer flange.

**Steg 208.** Implementer bends.

**Steg 209.** Implementer corner/bend relief.

**Steg 210.** Implementer unfold/refold.

**Steg 211.** Generer flat pattern.

**Steg 212.** Exporter flat pattern som DXF.

Fusion bruker tilsvarende konsept med regler, bends/flanges og flat-pattern-relaterte arbeidsflyter. citeturn1search4

### Native prosjektformat

**Steg 213.** Design formatet før store modeller eksisterer.

**Steg 214.** Legg inn:

```text
FormatVersion
ApplicationVersion
KernelVersion
CreatedWith
Units
FeatureSchemaVersion
```

**Steg 215.** Lag atomisk save:

```text
document.tmp
→ fsync/flush
→ validate
→ rename to document.cadx
```

**Steg 216.** Aldri overskriv eneste kopi før ny kopi er bekreftet.

**Steg 217.** Lag periodisk recovery journal.

**Steg 218.** Lag backward migration:

```text
Schema 1 → 2
Schema 2 → 3
...
```

**Steg 219.** Aldri la en intern refactor automatisk ødelegge gamle dokumenter.

### Testing

**Steg 220.** Hver kernel-operasjon skal ha unit tests.

**Steg 221.** Lag analytic tests:

```text
Box volume
Cylinder volume
Sphere volume
Plane intersections
Circle radius
```

**Steg 222.** Lag pathological tests:

```text
almost tangent
almost coincident
tiny edge
huge dimensions
very small dimensions
nearly parallel faces
self-intersection
```

**Steg 223.** Lag Boolean regression corpus.

**Steg 224.** Lag fillet regression corpus.

**Steg 225.** Lag import corpus.

**Steg 226.** Lag minst én golden model per filformat.

**Steg 227.** Test assemblies med nested instances.

**Steg 228.** Test documents med tusenvis av features.

**Steg 229.** Test cancellation mid-import.

**Steg 230.** Test corrupted files.

**Steg 231.** Test disk-full under save.

**Steg 232.** Test GPU device lost.

**Steg 233.** Test native-kernel exception.

**Steg 234.** Test abnormal process termination.

**Steg 235.** Kjør eksport→import-roundtrip automatisk i CI for standardformatene.

### Performance

**Steg 236.** Instrumenter alt før optimalisering.

Mål:

```text
Sketch solve time
Feature recompute time
Kernel Boolean time
Tessellation time
GPU upload time
Frame time
Import time
Export time
Peak RAM
```

**Steg 237.** Cache features basert på revision/hash.

**Steg 238.** Bruk parallel processing kun der kernel og importer faktisk støtter det. OCCTs Data Exchange Wrapper dokumenterer ulik thread-safety mellom formatproviderne; for eksempel er STEP per-reader thread-safe mens IGES er oppført annerledes. citeturn16search4

**Steg 239.** Ikke recompute urelatert geometri.

**Steg 240.** Ikke retesseller hele documentet etter én dimensjonsendring.

**Steg 241.** Frustum-cull store assemblies.

**Steg 242.** Bruk instanced drawing for repeterte component instances.

**Steg 243.** Unload visual meshes for skjulte massive assemblies ved behov.

### Stabil release

**Steg 244.** Lag signed x64 Windows build.

**Steg 245.** Pakk native OCCT DLL-er kontrollert.

**Steg 246.** Ikke last vilkårlige DLL-er fra working directory.

**Steg 247.** Implementer plug-in signing/trust policy.

**Steg 248.** Lag `Safe Mode`:

```text
No third-party plug-ins
Software rendering allowed
Default settings
No recovered workspace state
```

**Steg 249.** Lag startup crash detection.

**Steg 250.** Dersom forrige startup krasjet, tilby Safe Mode.

**Steg 251.** Lag user-facing diagnostics package uten å inkludere CAD-modellen dersom brukeren ikke eksplisitt tillater det.

**Steg 252.** Gjør telemetry valgfri eller helt fraværende dersom målet er et rent offline-produkt.

## Stabilitet, NVIDIA-rendering og personalisering

### Rendererens arbeidsdeling

GPU-en bør gjøre:

```text
vertices
triangles
normals
edges
materials
lighting
selection visualization
occlusion
anti-aliasing
post-processing
```

CPU/CAD-kjernen bør gjøre:

```text
exact B-Rep
NURBS evaluation
Booleans
intersections
fillets
topology
constraints
feature recompute
manufacturing validity
```

Dette skillet følger den grunnleggende CAD-arkitekturen OCCT selv muliggjør ved å holde eksakt shape-geometri separat fra trianguleringen. citeturn10search3

OCCTs egen desktop-visualisering forventer GPU/OpenGL-capabilities og tilbyr blant annet real-time PBR, som ytterligere bekrefter at en moden CAD-stack bør bruke GPU-rendering uten at GPU-meshet blir modellens geometriske sannhet. citeturn12search3turn10search39

### GPU-innstillinger brukeren bør få

```text
Settings > Graphics

Renderer:
    Direct3D 11
    Direct3D 12 Experimental

GPU:
    Automatic High Performance
    NVIDIA GeForce RTX ...
    Integrated GPU
    WARP fallback

Frame rate:
    30
    60
    120
    Unlimited

Anti-aliasing:
    Off
    2x
    4x
    8x

Tessellation:
    Adaptive
    Low
    Medium
    High
    Ultra
    Custom

Curve quality:
    Low → Ultra

Edge display:
    Visible
    Hidden
    Silhouette
    Tangent edges

Ambient occlusion:
    Off / Low / Medium / High

PBR:
    On / Off

Transparency quality:
    Fast / Accurate

Large assembly mode:
    Automatic / Manual

GPU memory budget:
    Automatic / Custom
```

DXGI/DXCore tilbyr Windows-mekanismer for å velge adapter etter performance preference, slik at `Automatic High Performance` kan velge diskret GPU på systemer hvor Windows klassifiserer den slik. citeturn12search1turn12search8

### CAD-presisjonsinnstillinger

Dette bør få en egen avansert side:

```text
Settings > Precision

Display
    Length decimal places
    Angle decimal places
    Scientific notation threshold
    Trailing zeros
    Fractional inches

Modeling
    Linear tolerance
    Angular tolerance

Sketch
    Constraint tolerance
    Snap tolerance
    Auto-constraint tolerance

Import
    Automatic healing
    Sewing tolerance
    Maximum allowed healing tolerance
    Reject invalid solids
    Preserve source units
    Generate health report

Export
    Geometry validation before export
    STEP protocol
    Sewing tolerance
    Include colors
    Include names
    Include assemblies
    Include PMI where supported

Mesh
    Chord tolerance
    Angular deflection
    Minimum edge
    Maximum edge
```

Autodesk sine egne beskrivelser av loose tolerances viser hvorfor import/healing og normal modeling precision ikke bør være samme verdi. citeturn4view5

### Navigation personalization

Brukeren bør kunne velge navigasjonsprofiler:

```text
Fusion-like
SolidWorks-like
Inventor-like
Blender-like
Custom
```

Alle mouse inputs bør være konfigurerbare:

```text
Orbit
Pan
Zoom
Select
Multi-select
Context menu
Fit
Pivot
```

### UI-personalisering

```text
Theme
    Light
    Dark
    System
    High Contrast

UI density
    Compact
    Normal
    Touch-friendly

Toolbar
    Custom tabs
    Custom groups
    Rearrange commands
    Hide commands

Browser
    Left
    Right
    Auto-hide

Timeline
    Bottom
    Hidden
    Height

View Cube
    Size
    Position
    Opacity

Selection colors
Grid colors
Sketch colors
Constraint colors
Background gradient
Edge colors
```

### Keyboard og command customization

Hver command bør registreres slik:

```csharp
CommandDefinition
{
    Id
    DisplayName
    Category
    DefaultShortcut
    IconId
    CanExecute
    Execute
}
```

Så kan brukeren selv mappe:

```text
E → Extrude
F → Fillet
H → Hole
M → Measure
L → Line
C → Circle
D → Sketch Dimension
Shift+S → Custom command
```

### Autosave og recovery

Programmet bør ikke bare «autosave hvert femte minutt».

Bruk:

```text
User Action
   ↓
Transaction Journal
   ↓
Periodic recovery snapshot
   ↓
Normal explicit save
```

Hvis programmet krasjer:

```text
Original document
+
Recovery journal
=
Recovered unsaved document
```

Dette er særlig viktig fordi native CAD-kernels og tredjepartsimportører introduserer feilmodi som et rent managed C#-program ikke ville hatt.

### Importører må isoleres

Spesielt proprietære filer kan være store, komplekse og potensielt korrupte. Derfor bør filoversettelse kunne foregå i:

```text
CadTranslatorWorker.exe
```

med memory/time limits og IPC.

Importer API:

```text
Main application
      ↓
Start worker
      ↓
Worker parses foreign CAD
      ↓
Convert → normalized exchange representation
      ↓
Validate
      ↓
Transfer into document
```

Dette betyr at en translator-crash normalt ikke tar med seg brukerens åpne design.

### «Manufacturing Ready»-status

Jeg ville lagt inn en svært synlig statusindikator:

```text
MODEL STATUS

● Geometry valid
● Closed manifold solid
● No self intersections
● No open boundaries
● Tolerances within profile
● Units defined
● Critical dimensions valid
● STEP round-trip passed

Manufacturing readiness:
PASS
```

eller:

```text
Manufacturing readiness:
FAIL

2 open edges
1 invalid face
Maximum imported tolerance exceeds project profile
```

Det er et område hvor programmet potensielt kan være **bedre og tydeligere enn Fusion** fremfor bare å kopiere arbeidsflyten. Autodesk selv beskriver hvordan løse toleranser og små/ambisiøse topologiske detaljer kan skape senere modelleringsfeil, mens OCCT har dedikerte validation- og shape-healing-verktøy. citeturn4view5turn11search22turn10search35

### Prioritert release-rekkefølge

Den mest fornuftige utviklingsrekkefølgen er:

| Release | Mål |
|---|---|
| **Prototype** | OCCT bridge + viewer + box/extrude/Boolean |
| **Alpha CAD** | Sketcher + constraints + parameters + timeline |
| **Core CAD** | Extrude/revolve/fillet/chamfer/hole/pattern/shell |
| **Precision CAD** | Validation + healing + robust topology refs |
| **Exchange** | STEP/IGES/STL/OBJ/3MF/DXF |
| **Production** | STEP round-trip + manufacturing checks |
| **Assembly** | Components + joints + interference |
| **Advanced Design** | Loft/sweep/surfaces/sheet metal |
| **Professional Exchange** | DWG/Inventor/SW/CATIA/Creo/NX/JT/Parasolid/ACIS |
| **Advanced Visualization** | PBR, large assemblies, refined GPU pipeline |
| **Extended Fusion parity** | Drawing, Form/SubD, CAM, simulation modules |

Det viktigste er at **«100 filformater» aldri får høyere prioritet enn at én enkelt cylinder blir representert, modifisert, lagret og eksportert korrekt**.

Fusion selv er blitt et omfattende system som spenner over CAD, CAM, CAE og elektronikk, så full funksjonsparitet er et flerårig produktprogram snarere enn én enkelt implementeringsoppgave. citeturn1search13turn17search11

Den mest solide tekniske basen jeg ville låst prosjektet til er derfor:

```text
Language/runtime       C# / .NET 10 LTS
Desktop UI             WPF
Exact CAD kernel       Open CASCADE Technology
Native interface       Narrow C ABI
Renderer               Direct3D via Vortice.Windows
GPU selection          DXGI/DXCore HighPerformance + manual selection
Sketch solver          Dedicated C# subsystem
Solver references      FreeCAD PlaneGCS + CADability architecture
Native document        Own versioned .cadx-style format
STEP/IGES              OCCT
STL/OBJ                 OCCT
3MF                     lib3mf
3DM                     openNURBS
DXF                     C# provider/CADability family
DWG                     ODA or HOOPS
Proprietary MCAD        HOOPS Exchange / appropriate licensed providers
USDZ                    OpenUSD adapter
F3D/F3Z/WIRE            gated provider until legitimate translator exists
Save/recovery           transaction journal + atomic files
Manufacturing safety   continuous B-Rep validation + round-trip tests
```

OCCT gir det nødvendige eksakte CAD-fundamentet; FreeCAD viser at en full parametrisk desktop-CAD kan bygges rundt denne typen kernel; CADability gir svært relevant C#-CAD-kildekode å studere; lib3mf og openNURBS dekker viktige åpne formater; HOOPS/ODA løser den delen av filformatproblemet som åpen kildekode realistisk ikke dekker godt nok; og DirectX/DXGI gir en ren Windows/NVIDIA-orientert renderingarkitektur. citeturn14view3turn4view2turn18search24turn15search12turn14search1turn20view0turn14view1turn12search0turn12search1