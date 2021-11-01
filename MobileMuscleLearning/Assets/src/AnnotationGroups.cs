using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnnotationGroups : MonoBehaviour {

    public class AnatomyObject
    {
        public AnatomyObject(GameObject annotation, RenderOption meshObject, string displayName)
        {
            Annotation = annotation;
            MeshObject = meshObject;
            DisplayName = displayName;
        }
        public GameObject Annotation;
        public RenderOption MeshObject;
        public string DisplayName;
    }

    public class ObjectGroup
    {

        public ObjectGroup(AnatomyObject goPrimary, AnatomyObject goSecond, AnatomyObject goThird, AnatomyObject goFourth)
        {
            primary = goPrimary;
            second = goSecond;
            third = goThird;
            fourth = goFourth;
        }
        public AnatomyObject primary = null;
        public AnatomyObject second = null;
        public AnatomyObject third = null;
        public AnatomyObject fourth = null;
    }

    public TextMeshProUGUI ToggleLeftRightText = null;
    public bool isLeft = true;
    public string prefixAnnotation = "AnatomyModel/AnnotationSkeleton/Armature/RootBone/";
    public string prefixModel = "AnatomyModel/MuscularySystem/";

    private Dictionary<int, AnatomyObject> id2GO;
    private const int rightOffset = 39;

    private List<ObjectGroup> groupsLeft = null;
    private List<ObjectGroup> groupsRight = null;
    private int groupCounter = 0;
    public bool selectiveRendering = false;

	// Use this for initialization
	void Start () {
        initDict();

        groupsLeft = new List<ObjectGroup>();
        groupsRight = new List<ObjectGroup>();
        groupsLeft.Add(new ObjectGroup(id2GO[14], id2GO[16], id2GO[18], id2GO[19]));    groupsRight.Add(new ObjectGroup(id2GO[14 + rightOffset], id2GO[16 + rightOffset], id2GO[18 + rightOffset], id2GO[19 + rightOffset]));
        groupsLeft.Add(new ObjectGroup(id2GO[13], id2GO[1], id2GO[2], null));           groupsRight.Add(new ObjectGroup(id2GO[13 + rightOffset], id2GO[1 + rightOffset], id2GO[2 + rightOffset], null));
        groupsLeft.Add(new ObjectGroup(id2GO[17], id2GO[21], id2GO[22], id2GO[23]));    groupsRight.Add(new ObjectGroup(id2GO[17 + rightOffset], id2GO[21 + rightOffset], id2GO[22 + rightOffset], id2GO[23 + rightOffset]));
        groupsLeft.Add(new ObjectGroup(id2GO[15], id2GO[4], id2GO[7], null));           groupsRight.Add(new ObjectGroup(id2GO[15 + rightOffset], id2GO[4 + rightOffset], id2GO[7 + rightOffset], null));
        groupsLeft.Add(new ObjectGroup(id2GO[5], id2GO[8], id2GO[10], null));           groupsRight.Add(new ObjectGroup(id2GO[5 + rightOffset], id2GO[8 + rightOffset], id2GO[10 + rightOffset], null));
        groupsLeft.Add(new ObjectGroup(id2GO[6], id2GO[12], id2GO[11], id2GO[9]));      groupsRight.Add(new ObjectGroup(id2GO[6 + rightOffset], id2GO[12 + rightOffset], id2GO[11 + rightOffset], id2GO[9 + rightOffset]));
        groupsLeft.Add(new ObjectGroup(id2GO[27], id2GO[28], id2GO[20], null));         groupsRight.Add(new ObjectGroup(id2GO[27 + rightOffset], id2GO[28 + rightOffset], id2GO[20 + rightOffset], null));
        groupsLeft.Add(new ObjectGroup(id2GO[26], id2GO[34], id2GO[33], null));         groupsRight.Add(new ObjectGroup(id2GO[26 + rightOffset], id2GO[34 + rightOffset], id2GO[33 + rightOffset], null));
        groupsLeft.Add(new ObjectGroup(id2GO[25], id2GO[27], null, null));              groupsRight.Add(new ObjectGroup(id2GO[25 + rightOffset], id2GO[27 + rightOffset], null, null));
        groupsLeft.Add(new ObjectGroup(id2GO[24], id2GO[31], id2GO[29], id2GO[30]));    groupsRight.Add(new ObjectGroup(id2GO[24 + rightOffset], id2GO[31 + rightOffset], id2GO[29 + rightOffset], id2GO[30 + rightOffset]));
        groupsLeft.Add(new ObjectGroup(id2GO[36], id2GO[39], id2GO[38], null));         groupsRight.Add(new ObjectGroup(id2GO[36 + rightOffset], id2GO[39 + rightOffset], id2GO[38 + rightOffset], null));
        groupsLeft.Add(new ObjectGroup(id2GO[35], id2GO[37], id2GO[38], null));         groupsRight.Add(new ObjectGroup(id2GO[35 + rightOffset], id2GO[37 + rightOffset], id2GO[38 + rightOffset], null));

        setAllMeshesOpaque();
    }

    private void Update()
    {
        //Camera.main.transform.position = (camPosition[groupCounter] + Camera.main.transform.position) / 10f;
    }

    public void setAllMeshesTransparent()
    {
        RenderOption[] ros = GameObject.FindObjectsOfType<RenderOption>();
        int numROS = ros.Length;
        for(int i=0;i<numROS;i++)
        {
            ros[i].Alpha = 0.015f;
            ros[i].Emmision = Color.white;
        }
    }

    public void setAllMeshesOpaque()
    {
        selectiveRendering = false;
        RenderOption[] ros = GameObject.FindObjectsOfType<RenderOption>();
        int numROS = ros.Length;
        for (int i = 0; i < numROS; i++)
        {
            ros[i].Alpha = 1f;
            ros[i].Emmision = Color.white;
        }
        
    }

    public AnatomyObject getPrimaryObject()
    {
        if (groupsLeft == null || groupsRight == null || !selectiveRendering) return null;
        return isLeft ? groupsLeft[groupCounter].primary : groupsRight[groupCounter].primary;
    }

    public AnatomyObject getSecondObject()
    {
        if (groupsLeft == null || groupsRight == null || !selectiveRendering) return null;
        return isLeft ? groupsLeft[groupCounter].second : groupsRight[groupCounter].second;
    }

    public AnatomyObject getThirdObject()
    {
        if (groupsLeft == null || groupsRight == null || !selectiveRendering) return null;
        return isLeft ? groupsLeft[groupCounter].third : groupsRight[groupCounter].third;
    }

    public AnatomyObject getFourthObject()
    {
        if (groupsLeft == null || groupsRight == null || !selectiveRendering)  return null;
        return isLeft ? groupsLeft[groupCounter].fourth : groupsRight[groupCounter].fourth;
    }

    //ID should be > 0 and < #groups
    public void setGroupOpaque(int id)
    {
        if (groupsLeft == null || groupsRight == null) return;

        selectiveRendering = true;

        int numGroups = groupsLeft.Count;
        groupCounter = id;
        setAllMeshesTransparent();

        AnatomyObject ao = null;
        if ((ao = isLeft ? groupsLeft[groupCounter].primary : groupsRight[groupCounter].primary) != null)
        {
            ao.MeshObject.Alpha = 1;
        }
        if ((ao = isLeft ? groupsLeft[groupCounter].second : groupsRight[groupCounter].second) != null)
        {
            ao.MeshObject.Alpha = 1;
            ao.MeshObject.Emmision = new Color(0, 0.5f, 0.25f);
        }
        if ((ao = isLeft ? groupsLeft[groupCounter].third : groupsRight[groupCounter].third) != null)
        {
            ao.MeshObject.Alpha = 1;
            ao.MeshObject.Emmision = new Color(0, 0.25f, 0.5f);
        }
        if ((ao = isLeft ? groupsLeft[groupCounter].fourth : groupsRight[groupCounter].fourth) != null)
        {
            ao.MeshObject.Alpha = 1;
            ao.MeshObject.Emmision = new Color(0.7f, 0.7f, 1f);
        }

    }

    public void ToggleLeftRight()
    {
        if(selectiveRendering)
        {
            isLeft = !isLeft;
            if (ToggleLeftRightText)
            {
                ToggleLeftRightText.text = isLeft ? "Left" : "Right";
            }
            setGroupOpaque(groupCounter);
        }
    }

    private void initDict()
    {
        id2GO = new Dictionary<int, AnatomyObject>()
        {

            //Left Side
            {0, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/TransversusAbdominis"), GameObject.Find(prefixModel+"Abdomen_TranAbdo").GetComponent<RenderOption>(), "Transversus Abdominis") },
            {1, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/RectusAbdominis"), GameObject.Find(prefixModel+"Abdomen_RectAbdo").GetComponent<RenderOption>(), "Rectus Abdominis") },
            {2, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/ExternalOblique"), GameObject.Find(prefixModel+"Abdomen_ExteObli").GetComponent<RenderOption>(), "External Oblique") },
            {3, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/InternalOblique.Left"), GameObject.Find(prefixModel+"Abdomen_InteObli_Left").GetComponent<RenderOption>(), "Internal Oblique") },
            {4, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/LatissimusDorsi.Left"), GameObject.Find(prefixModel+"Abdomen_LatDor_Left").GetComponent<RenderOption>(), "Latissimus Dorsi") },
            {5, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Trapez.Left"), GameObject.Find(prefixModel+"Abdomen_Trapez_Left").GetComponent<RenderOption>(), "Trapezius") },
            {6, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Sterno.Left"), GameObject.Find(prefixModel+"Neck_Sterno_Left").GetComponent<RenderOption>(), "Sternocleidomastoid") },
            {7, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/PectoralisMajor.Left"), GameObject.Find(prefixModel+"Abdomen_Pectoralis_Left").GetComponent<RenderOption>(), "Pectoralis Major") },
            {8, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Platysma.Left"), GameObject.Find(prefixModel+"Neck_Platysma_Left").GetComponent<RenderOption>(), "Platysma") },
            {9, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Chest/Head/Occipitalis.Left"), GameObject.Find(prefixModel+"Head_UpperHead_Occi_Left").GetComponent<RenderOption>(), "Occipitalis") },
            {10, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Chest/Head/Temporalis.Left"), GameObject.Find(prefixModel+"Head_Temporalis_Left").GetComponent<RenderOption>(), "Temporalis") },
            {11, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Chest/Head/Frontalis.Left"), GameObject.Find(prefixModel+"Head_UpperHead_Occi_Left").GetComponent<RenderOption>(), "Frontalis") },
            {12, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Chest/Head/Zygomaticus.Left"), GameObject.Find(prefixModel+"Head_Zygo_Left").GetComponent<RenderOption>(), "Zygomaticus") },
            {13, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Left/Triceps.Left"), GameObject.Find(prefixModel+"Arm_Triceps_Left").GetComponent<RenderOption>(), "Triceps Brachii") },
            {14, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Left/Biceps.Left"), GameObject.Find(prefixModel+"Arm_Biceps_Left").GetComponent<RenderOption>(), "Biceps Brachii") },
            {15, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Left/Deltoid.Left"), GameObject.Find(prefixModel+"Shoulder_Left").GetComponent<RenderOption>(), "Deltoid") },
            {16, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Left/Brachialis.Left"), GameObject.Find(prefixModel+"Arm_Brachialis_Left").GetComponent<RenderOption>(), "Brachialis") },
            {17, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Left/Elbow.Left/ExtCarRadLon.Left"), GameObject.Find(prefixModel+"LowerArm_ExtCarpiRadialLongus_Left").GetComponent<RenderOption>(), "Extensor Carpi Radialis Longus") },
            {18, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Left/Elbow.Left/Brachior.Left"), GameObject.Find(prefixModel+"Arm_Brachioradialis_Left").GetComponent<RenderOption>(), "Brachioradialis") },
            {19, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Left/Elbow.Left/PronatorTeres.Left"), GameObject.Find(prefixModel+"Arm_PronTere_Left").GetComponent<RenderOption>(), "Pronator Teres") },
            {20, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Left/Elbow.Left/FlexorCarpiRadialis.Left"), GameObject.Find(prefixModel+"Arm_FlexCarpRadi_Left").GetComponent<RenderOption>(), "Flexor Carpi Radialis") },
            {21, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Left/Elbow.Left/FlexorCarpiUlnaris.Left"), GameObject.Find(prefixModel+"Arm_FlexCarpUlna_Left").GetComponent<RenderOption>(), "Flexor Carpi Ulnaris") },
            {22, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Left/Elbow.Left/ExtensorCarpiUlnaris.Left"), GameObject.Find(prefixModel+"Arm_ExteCarpUlna_Left").GetComponent<RenderOption>(), "Extensor Carpi Ulnaris") },
            {23, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Left/Elbow.Left/ExtensorDigitorum.Left"), GameObject.Find(prefixModel+"Arm_ExteDigi_Left").GetComponent<RenderOption>(), "Extensor Digitorum") },
            {24, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Left/BicFem.Left"), GameObject.Find(prefixModel+"Leg_BicepsFemor_Left").GetComponent<RenderOption>(), "Biceps Femoris") },
            {25, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Left/VasMed.Left"), GameObject.Find(prefixModel+"Leg_VastMedial_Left").GetComponent<RenderOption>(), "Vastus Medialis") },
            {26, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Left/VasLat.Left"), GameObject.Find(prefixModel+"Leg_VastLateral_Left").GetComponent<RenderOption>(), "Vastus Lateralis") },
            {27, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Left/RecFem.Left"), GameObject.Find(prefixModel+"Leg_RectFemor_Left").GetComponent<RenderOption>(), "Rectus Femoris")},
            {28, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Left/Iliopsoas.Left"), GameObject.Find(prefixModel+"Leg_Iliop_Left").GetComponent<RenderOption>(), "Iliopsoas") },
            {29, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Left/GluteusMedius.Left"), GameObject.Find(prefixModel+"Leg_GlutMedi_Left").GetComponent<RenderOption>(), "Gluteus Medius") },
            {30, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Left/GluteusMaximus.Left"), GameObject.Find(prefixModel+"Leg_GlutMaxi_Left").GetComponent<RenderOption>(), "Gluteus Maximus") },
            {31, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Left/Semitendinosus.Left"), GameObject.Find(prefixModel+"Leg_Semitendinosus_Left").GetComponent<RenderOption>(), "Semitendinosus") },
            {32, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Left/Semimembranosus.Left"), GameObject.Find(prefixModel+"Leg_Semimembranosus_Left").GetComponent<RenderOption>(), "Semimembranosus") },
            {33, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Left/Sartorius.Left"), GameObject.Find(prefixModel+"Leg_Sart_Left").GetComponent<RenderOption>(), "Sartorius") },
            {34, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Left/AdductorLongus.Left"), GameObject.Find(prefixModel+"Leg_AdduLong_Left").GetComponent<RenderOption>(), "Adductor Longus") },
            {35, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Left/Knee.Left/Gastroc.Left"), GameObject.Find(prefixModel+"Leg_Gastrocnemius_Left").GetComponent<RenderOption>(), "Gastrocnemius") },
            {36, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Left/Knee.Left/TibAnt.Left"), GameObject.Find(prefixModel+"Leg_TibialAnterior_Left").GetComponent<RenderOption>(), "Tibialis Anterior") },
            {37, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Left/Knee.Left/Soleus.Left"), GameObject.Find(prefixModel+"Leg_Soleus_Left").GetComponent<RenderOption>(), "Soleus") },
            {38, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Left/Knee.Left/FibularisLongus.Left"), GameObject.Find(prefixModel+"Leg_FibuLong_Left").GetComponent<RenderOption>(), "Fibularis Longus") },
            {39, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Left/Knee.Left/ExtensorDigitorumLongus.Left"), GameObject.Find(prefixModel+"Leg_ExteDigiLong_Left").GetComponent<RenderOption>(), "Extensor Digitorum Longus") },

            //Right Side
            {40, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/RectusAbdominis"), GameObject.Find(prefixModel+"Abdomen_RectAbdo").GetComponent<RenderOption>(), "Rectus Abdominis") },
            {41, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/ExternalOblique"), GameObject.Find(prefixModel+"Abdomen_ExteObli").GetComponent<RenderOption>(), "External Oblique") },
            {42, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/InternalOblique.Right"), GameObject.Find(prefixModel+"Abdomen_InteObli_Right").GetComponent<RenderOption>(), "Internal Oblique") },
            {43, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/LatissimusDorsi.Right"), GameObject.Find(prefixModel+"Abdomen_LatDor_Right").GetComponent<RenderOption>(), "Latissimus Dorsi") },
            {44, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Trapez.Right"), GameObject.Find(prefixModel+"Abdomen_Trapez_Right").GetComponent<RenderOption>(), "Trapezius") },
            {45, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Sterno.Right"), GameObject.Find(prefixModel+"Neck_Sterno_Right").GetComponent<RenderOption>(), "Sternocleidomastoid") },
            {46, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/PectoralisMajor.Right"), GameObject.Find(prefixModel+"Abdomen_Pectoralis_Right").GetComponent<RenderOption>(), "Pectoralis Major") },
            {47, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Platysma.Right"), GameObject.Find(prefixModel+"Neck_Platysma_Right").GetComponent<RenderOption>(), "Platysma") },
            {48, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Chest/Head/Occipitalis.Right"), GameObject.Find(prefixModel+"Head_UpperHead_Occi_Right").GetComponent<RenderOption>(), "Occipitalis") },
            {49, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Chest/Head/Temporalis.Right"), GameObject.Find(prefixModel+"Head_Temporalis_Right").GetComponent<RenderOption>(), "Temporalis") },
            {50, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Chest/Head/Frontalis.Right"), GameObject.Find(prefixModel+"Head_UpperHead_Occi_Right").GetComponent<RenderOption>(), "Frontalis") },
            {51, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Chest/Head/Zygomaticus.Right"), GameObject.Find(prefixModel+"Head_Zygo_Right").GetComponent<RenderOption>(), "Zygomaticus") },
            {52, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Right/Triceps.Right"), GameObject.Find(prefixModel+"Arm_Triceps_Right").GetComponent<RenderOption>(), "Triceps Brachii") },
            {53, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Right/Biceps.Right"), GameObject.Find(prefixModel+"Arm_Biceps_Right").GetComponent<RenderOption>(), "Biceps Brachii") },
            {54, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Right/Deltoid.Right"), GameObject.Find(prefixModel+"Shoulder_Right").GetComponent<RenderOption>(), "Deltoid") },
            {55, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Right/Brachialis.Right"), GameObject.Find(prefixModel+"Arm_Brachialis_Right").GetComponent<RenderOption>(), "Brachialis") },
            {56, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Right/Elbow.Right/ExtCarRadLon.Right"), GameObject.Find(prefixModel+"LowerArm_ExtCarpiRadialLongus_Right").GetComponent<RenderOption>(), "Extensor Carpi Radialis Longus") },
            {57, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Right/Elbow.Right/Brachior.Right"), GameObject.Find(prefixModel+"Arm_Brachioradialis_Right").GetComponent<RenderOption>(), "Brachioradialis") },
            {58, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Right/Elbow.Right/PronatorTeres.Right"), GameObject.Find(prefixModel+"Arm_PronTere_Right").GetComponent<RenderOption>(), "Pronator Teres") },
            {59, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Right/Elbow.Right/FlexorCarpiRadialis.Right"), GameObject.Find(prefixModel+"Arm_FlexCarpRadi_Right").GetComponent<RenderOption>(), "Flexor Carpi Radialis") },
            {60, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Right/Elbow.Right/FlexorCarpiUlnaris.Right"), GameObject.Find(prefixModel+"Arm_FlexCarpUlna_Right").GetComponent<RenderOption>(), "Flexor Carpi Ulnaris") },
            {61, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Right/Elbow.Right/ExtensorCarpiUlnaris.Right"), GameObject.Find(prefixModel+"Arm_ExteCarpUlna_Right").GetComponent<RenderOption>(), "Extensor Carpi Ulnaris") },
            {62, new AnatomyObject(GameObject.Find(prefixAnnotation+"Base/Abdomen/Shoulder.Right/Elbow.Right/ExtensorDigitorum.Right"), GameObject.Find(prefixModel+"Arm_ExteDigi_Right").GetComponent<RenderOption>(), "Extensor Digitorum") },
            {63, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Right/BicFem.Right"), GameObject.Find(prefixModel+"Leg_BicepsFemor_Right").GetComponent<RenderOption>(), "Biceps Femoris") },
            {64, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Right/VasMed.Right"), GameObject.Find(prefixModel+"Leg_VastMedial_Right").GetComponent<RenderOption>(), "Vastus Medialis") },
            {65, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Right/VasLat.Right"), GameObject.Find(prefixModel+"Leg_VastLateral_Right").GetComponent<RenderOption>(), "Vastus Lateralis") },
            {66, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Right/RecFem.Right"), GameObject.Find(prefixModel+"Leg_RectFemor_Right").GetComponent<RenderOption>(), "Rectus Femoris")},
            {67, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Right/Iliopsoas.Right"), GameObject.Find(prefixModel+"Leg_Iliop_Right").GetComponent<RenderOption>(), "Iliopsoas") },
            {68, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Right/GluteusMedius.Right"), GameObject.Find(prefixModel+"Leg_GlutMedi_Right").GetComponent<RenderOption>(), "Gluteus Medius") },
            {69, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Right/GluteusMaximus.Right"), GameObject.Find(prefixModel+"Leg_GlutMaxi_Right").GetComponent<RenderOption>(), "Gluteus Maximus") },
            {70, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Right/Semitendinosus.Right"), GameObject.Find(prefixModel+"Leg_Semitendinosus_Right").GetComponent<RenderOption>(), "Semitendinosus") },
            {71, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Right/Semimembranosus.Right"), GameObject.Find(prefixModel+"Leg_Semimembranosus_Right").GetComponent<RenderOption>(), "Semimembranosus") },
            {72, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Right/Sartorius.Right"), GameObject.Find(prefixModel+"Leg_Sart_Right").GetComponent<RenderOption>(), "Sartorius") },
            {73, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Right/AdductorLongus.Right"), GameObject.Find(prefixModel+"Leg_AdduLong_Right").GetComponent<RenderOption>(), "Adductor Longus") },
            {74, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Right/Knee.Right/Gastroc.Right"), GameObject.Find(prefixModel+"Leg_Gastrocnemius_Right").GetComponent<RenderOption>(), "Gastrocnemius") },
            {75, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Right/Knee.Right/TibAnt.Right"), GameObject.Find(prefixModel+"Leg_TibialAnterior_Right").GetComponent<RenderOption>(), "Tibialis Anterior") },
            {76, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Right/Knee.Right/Soleus.Right"), GameObject.Find(prefixModel+"Leg_Soleus_Right").GetComponent<RenderOption>(), "Soleus") },
            {77, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Right/Knee.Right/FibularisLongus.Right"), GameObject.Find(prefixModel+"Leg_FibuLong_Right").GetComponent<RenderOption>(), "Fibularis Longus") },
            {78, new AnatomyObject(GameObject.Find(prefixAnnotation+"Hip.Right/Knee.Right/ExtensorDigitorumLongus.Right"), GameObject.Find(prefixModel+"Leg_ExteDigiLong_Right").GetComponent<RenderOption>(), "Extensor Digitorum Longus") }
        };

    }
}
