using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/*
Bien. Tú, estimado corrector del proyecto de investigacion, si de casualidad has llegado hasta aqui,
te preguntarás: Acaso puede este alumno mantener su vida social y a la vez desarrollar esto?
digo, eeeh, que es lo que hace esta maravillosa pieza de codigo?

Bueno, pues va a hacer lo siguente:

Partimos de un NOMBRE DE PROYECTO y un PROGRESO dados del WindowManager
De momento son valores que no tienen mucha influencia a la hora de cambiar de proyecto, ya que todos
en realidad son el mismo (en esta demo). A futuro se puede añadir a la estructura Proyect el archivo
de los apuntes, y pasarlo al servidor.

hay dos estados de quiz, GENERAL y FEEDBACK
durante el modo GENERAL, haremos un quiz general al alumno, y estableceremos su progreso total al
resultado de este quiz. Generaremos un feedback, y los topics devueltos se guardarán en una lista.

Después de completar la fase GENERAL, iremos generando quizzes de feedback de cada topic. Le sumaremos
a proggress la puntuacion del quiz, dividida en el numero de topics que hay en la lista, y ajustado para
que llene el porcentaje que falta del quiz general.

Una vez acabada la fase FEEDBACK, volveremos a la fase GENERAL y repetiremos, hasta que el alumno obtenga
el 100% de la puntuacion.

*/

//clases del quiz
[System.Serializable]
public class Answer
{
    public string answer;
}

[System.Serializable]
public class Question
{
    public string question;
    public int correct;
    public Answer[] answers;
}

[System.Serializable]
public class Quiz
{
    public string quiz_name;
    public Question[] questions;
}

public class FeedbackWrapper
{
    public List<string> feedbackList;
}
//enum para las ventanas
public enum ProyectWindow
{
    Question,
    QuizEnd,
    RankUp,
    Loading,
    None
}
public class ProyectManager : MonoBehaviour
{
    private string hostUrl = "http://localhost:9033";
    public Proyect proyect;
    public WindowManager windowManager;
    public bool isGeneral = true;
    public List<string> feedbackTopics;
    public int totalFeedbackTopics;
    public float generalProgrees;


    [Header("Current Quiz")]
    public Quiz currentQuiz;

    public string currentQuizJson;

    public int currentCuestionIdx;
    public int correctAnswers;
    public List<int> answers;

    [Header("UI")]
    public ProyectWindow currentWindow;
    public GameObject questionWindow;
    public GameObject quizEndWindow;
    public GameObject loadingWindow;
    public GameObject rankUpWindow;
    public GameObject descansoWindow;
    [Header("Question Window")]
    public TMP_Text uiQuestion;
    public TMP_Text[] uiAnswers;
    public TMP_Text questNumber;
    public TMP_Text questPercentage;
    public Image progressBar;
    [Header("Quiz End Window")]
    public TMP_Text endGrade;
    public TMP_Text rankProgress;
    public GameObject rankProgressArrow;
    public RankIndicator endRank;
    public Button continueButton;
    public GameObject continueWaitIndicator;
    public TMP_Text feedbackMessage;
    private string originalFeedbackMessage;

    [Header("Uprank Window")]
    public RankIndicator newRank;
    [Header("Quiz History")]
    public SimpleListManager historyList;



    // Start is called before the first frame update
    void Start()
    {
        originalFeedbackMessage = feedbackMessage.text;
        descansoWindow.SetActive(false);
        SetWindow(ProyectWindow.None);

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void nextQuiz()
    {
        descansoWindow.SetActive(false);

        currentCuestionIdx = 0;
        correctAnswers = 0;
        answers = new();

        if (isGeneral)
        {
            //general quiz
            GetGeneralQuizFromServer(); //we'll wait for it to generate, while we'll show the loading screen
            SetWindow(ProyectWindow.Loading);
        }
        else
        {
            if (feedbackTopics.Count > 0)
            {
                string topic = feedbackTopics[0];
                feedbackTopics.Remove(topic);

                //generate quiz with topic
                StartCoroutine(GetCustomQuizData(topic)); //we'll wait for it to generate, while we'll show the loading screen
                SetWindow(ProyectWindow.Loading);
            }
            else
            {
                //general quiz again
                isGeneral = true;
                nextQuiz();
            }
        }

    }

    public void LoadQuestion()
    {
        Question q = currentQuiz.questions[currentCuestionIdx];

        uiQuestion.text = q.question;
        for (int i = 0; i < 4; i++)
        {
            if (i >= q.answers.Length)
            {
                uiAnswers[i].transform.parent.gameObject.SetActive(false);
            }
            else
            {
                uiAnswers[i].transform.parent.gameObject.SetActive(true);
                uiAnswers[i].text = q.answers[i].answer;
            }
        }

        float perc = currentCuestionIdx / (float)currentQuiz.questions.Length;

        string qnum = (currentCuestionIdx + 1).ToString() + "/" + currentQuiz.questions.Length.ToString();
        string sperc = ((int)(perc * 100f)).ToString() + "%";

        questNumber.text = qnum;
        questPercentage.text = sperc;

        progressBar.fillAmount = perc;
    }

    public void AnswerQuestion(int idx)
    {
        Question q = currentQuiz.questions[currentCuestionIdx];
        answers.Add(idx);
        if (q.correct == idx)
        {
            correctAnswers += 1;
        }
        currentCuestionIdx++;
        if (currentCuestionIdx >= currentQuiz.questions.Length)
        {
            //check here if we've finished sesion
            if (!isGeneral && feedbackTopics.Count == 0)
            {
                SetWindow(ProyectWindow.None);

                descansoWindow.SetActive(true);
            }
            else
            {

                QuizEnd();
            }

        }
        else
        {
            LoadQuestion();
        }
    }

    public void QuizEnd()
    {
        SetWindow(ProyectWindow.QuizEnd);

        descansoWindow.SetActive(false);

        endGrade.text = correctAnswers.ToString() + "/" + currentQuiz.questions.Length.ToString() +
        "\n\n" + ((int)(correctAnswers / (float)currentQuiz.questions.Length * 100f)).ToString() + "%";


        //calculate new grade
        float previousProggress = proyect.proggress;
        float perc = correctAnswers / (float)currentQuiz.questions.Length;
        if (isGeneral)
        {
            proyect.proggress = perc * 100f;
        }
        else
        {
            proyect.proggress += perc * (100f - generalProgrees) / totalFeedbackTopics;
        }

        endRank.Progress = proyect.proggress;

        continueWaitIndicator.SetActive(true);
        continueButton.interactable = false;
        print("Prev prog: " + previousProggress);
        if (!float.IsNaN(previousProggress))
        {
            rankProgressArrow.SetActive(true);
            int rm = (int)(proyect.proggress - previousProggress) * 5;
            string rankMovement;
            if (rm >= 0)
            {
                rankMovement = "+";
                rankProgressArrow.transform.rotation = new();
            }
            else
            {
                rankMovement = "";
                rankProgressArrow.transform.rotation = new();
                rankProgressArrow.transform.Rotate(new Vector3(0, 0, 180));
            }
            rankMovement += rm.ToString() + "%";

            rankProgress.text = rankMovement;

            if ((int)(previousProggress / 20) < (int)(proyect.proggress / 20) || (previousProggress < proyect.proggress && proyect.proggress == 100))
            {
                //new rank
                newRank.Progress = proyect.proggress;
                StartCoroutine(ShowNewRank());
            }
        }
        else
        {
            rankProgressArrow.SetActive(false);
            rankProgress.text = "";
        }


        feedbackMessage.text = originalFeedbackMessage;

        //get feedback if general.
        if (isGeneral)
        {
            StartCoroutine(SendFeedbackData());
            isGeneral = false;
            generalProgrees = proyect.proggress;
        }
        else
        {
            //wait for continue button -> nextQuiz
            continueWaitIndicator.SetActive(false);
            continueButton.interactable = true;

            if (feedbackTopics.Count > 0)
            {
                feedbackMessage.text = "Quedan " + feedbackTopics.Count + " rondas de repaso.";
            }
            else
            {
                feedbackMessage.text = "Preparado para otra ronda general?";
            }
        }
    }
    public void GenerateFeedbackEnd()
    {
        //wait for continue button -> nextQuiz
        continueWaitIndicator.SetActive(false);
        continueButton.interactable = true;
        feedbackMessage.text = "Generadas " + feedbackTopics.Count + " rondas de repaso.";
        totalFeedbackTopics = feedbackTopics.Count;
    }

    public IEnumerator ShowNewRank()
    {
        SetWindow(ProyectWindow.RankUp);
        yield return new WaitForSeconds(3f);
        SetWindow(ProyectWindow.QuizEnd);
    }

    public void GetGeneralQuizFromServer()
    {
        StartCoroutine(GetQuizData());
    }

    public void quizLoadingDone()
    {
        SetWindow(ProyectWindow.Question);
        LoadQuestion();
    }

    public void SetWindow(ProyectWindow window)
    {
        GameObject[] pages = { questionWindow, quizEndWindow, rankUpWindow, loadingWindow };
        foreach (var item in pages)
        {
            item.SetActive(false);
        }

        if (window != ProyectWindow.None) pages[(int)window].SetActive(true);
        currentWindow = window;
    }

    public void backToWindowManager(int window = 1)
    {
        descansoWindow.SetActive(false);

        SetWindow(ProyectWindow.None);
        windowManager.moveToWindow(window);
    }

    //SERVER LOADING STUFF
    private IEnumerator GetQuizData()
    {
        string apiUrl = hostUrl + "/generalQuiz";

        Debug.Log("Web request to: " + apiUrl);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {
            webRequest.timeout = 1000;
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Quiz request error: " + webRequest.error);
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                ParseJsonToQuiz(jsonResponse);
                currentQuizJson = jsonResponse;
                quizLoadingDone();
            }
        }
    }

    private IEnumerator GetCustomQuizData(string topic)
    {
        string safeTopic = UnityWebRequest.EscapeURL(topic);
        string apiUrl = hostUrl + "/customQuiz/" + safeTopic;

        Debug.Log("Web request to: " + apiUrl);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("FEEDBACK QUIZ SERVER ERROR " + webRequest.error);
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                ParseJsonToQuiz(jsonResponse);
                currentQuizJson = jsonResponse;
                quizLoadingDone();
            }
        }
    }

    private void ParseJsonToQuiz(string jsonString)
    {
        try
        {
            currentQuiz = JsonUtility.FromJson<Quiz>(jsonString);

            if (currentQuiz == null)
            {
                Debug.LogError("UNKNOWN JSON PARSE ERROR");
            }

            //log quiz  (we can do it here as we're not saving the contents yet)
            DateTime theTime = DateTime.Now;
            string datetime = theTime.ToString("yyyy-MM-dd\\THH:mm:ss\\Z");

            historyList.listContents.Add(currentQuiz.quiz_name + " - " + datetime);
            historyList.refreshSpawned();
        }
        catch (System.Exception e)
        {
            Debug.LogError("JSON PARSE ERROR: " + e.Message);
        }
    }

    private IEnumerator SendFeedbackData()
    {
        string apiUrl = hostUrl + "/feedback";

        string quizJson = currentQuizJson;

        string answersJson = "[" + string.Join(",", answers) + "]";


        WWWForm form = new WWWForm();
        form.AddField("jsonq", quizJson);
        form.AddField("answers", answersJson);

        Debug.Log("Sending feedback to server");

        //send post
        using (UnityWebRequest webRequest = UnityWebRequest.Post(apiUrl, form))
        {
            webRequest.timeout = 1000;
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("SERVER CONNECT ERROR: " + webRequest.error);
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("[FB] RAW SERVER RESPONSE: " + jsonResponse);

                string wrappedJson = "{\"feedbackList\":" + jsonResponse + "}";

                try
                {
                    FeedbackWrapper wrapper = JsonUtility.FromJson<FeedbackWrapper>(wrappedJson);
                    feedbackTopics = wrapper.feedbackList;

                    GenerateFeedbackEnd();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[FB] PARSE ERROR: " + e.Message);
                }
            }
        }
    }
}
