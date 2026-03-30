using System.Collections;
using UnityEngine;
using TheSSand.Core;
using TheSSand.Audio;
using TheSSand.Scene;

namespace TheSSand.UI
{
    public class EndingController : MonoBehaviour
    {
        [SerializeField] GameObject grandpaRoom;
        [SerializeField] CanvasGroup fadeOverlay;

        void Start()
        {
            int ending = EndingManager.Instance?.EvaluateAndTriggerEnding() ?? 1;
            StartCoroutine(PlayEnding(ending));
        }

        IEnumerator PlayEnding(int ending)
        {
            AudioManager.Instance?.PlaySFX("SFX_BookPageTurn");

            yield return new WaitForSeconds(1f);

            switch (ending)
            {
                case 1: yield return Ed1(); break;
                case 2: yield return Ed2(); break;
                case 3: yield return Ed3(); break;
                case 4: yield return Ed4(); break;
                case 5: yield return Ed5(); break;
                default: yield return NGNormalEnding(); break;
            }
        }

        IEnumerator Ed1()
        {
            yield return ShowDialogueSequence(new[]
            {
                ("할아버지", "자, 이렇게 해서 용사는 세상을 구했단다."),
                ("할아버지", "네 개의 씨앗을 심고, 여행을 끝냈지."),
                ("할아버지", "어때, 재밌었니?"),
                ("손자", "응! 근데 용사는 그 다음에 어떻게 됐어요?"),
                ("할아버지", "그건… 그 다음 이야기가 있단다."),
                ("할아버지", "오늘은 여기까지란다. 다음에 또 들려주마.")
            });

            NarrationUI.Instance?.ShowNarrationSequence(new[]
            {
                "당신은 세상을 구했습니다.",
                "성공적으로 처음에 가져왔던 네 개의 씨앗을,\n네 개의 오아시스에 유치시켰습니다.",
                "이 씨앗으로 인해 오아시스가 어떻게 변할지는 모르지만,\n어쨌든 당신은 세상을 구했습니다."
            }, () => GoToCredits());
        }

        IEnumerator Ed2()
        {
            yield return ShowDialogueSequence(new[]
            {
                ("손자", "용사는 좋은 사람이었네요, 할아버지."),
                ("할아버지", "…"),
                ("할아버지", "그래.\n그렇지."),
                ("할아버지", "좋은 사람이었어.")
            });

            NarrationUI.Instance?.ShowNarrationSequence(new[]
            {
                "당신은 아이들을 사랑하는 용사입니다.",
                "아이들이 진흙쿠키를 굽고, 스스로 살아가는 과정을 지켜보면서\n당신이 이들의 보호자가 되어야겠다는 확신을 갖게 되었습니다.",
                "당신은 이 오아시스로 돌아와 아이들을 돌볼 것입니다.\n언제까진지는 모르지만요."
            }, () => GoToCredits());
        }

        IEnumerator Ed3()
        {
            yield return ShowDialogueSequence(new[]
            {
                ("손자", "할아버지… 그건 나쁜 거 아니에요?"),
                ("할아버지", "그건 말이다…"),
                ("할아버지", "오늘은 여기까지란다. 늦었어, 자야지.")
            });

            NarrationUI.Instance?.ShowNarrationSequence(new[]
            {
                "당신은 그들을 설득하지 않고 힘을 휘둘렀습니다.",
                "에잇, 뭐 어때요.\n당신은 힘이 있고, 저들은 힘이 없는 걸요.\n꼬우면 반격했어야죠.",
                "어차피 망할 세상에서 살아갈 그들을\n걱정할 필요가 뭐가 있답니까."
            }, () => GoToCredits());
        }

        IEnumerator Ed4()
        {
            yield return ShowDialogueSequence(new[]
            {
                ("손자", "황금우상이 그렇게 비싼 거예요?"),
                ("할아버지", "하하, 그거야."),
                ("할아버지", "반짝거리는 건 다 비싸 보이는 법이란다."),
                ("할아버지", "오늘은 여기까지.")
            });

            NarrationUI.Instance?.ShowNarrationSequence(new[]
            {
                "당신은 이 여행에서 세상을 구하고 자신 또한 구했습니다.",
                "그렇죠, 큰 깨달음을 하나 얻은 거죠.",
                "돈이 최고라는 깨달음 말입니다.",
                "이 반짝거리는 황금우상만 있다면",
                "당신이 원래 살던 곳으로 돌아가더라도 편하게 살 수 있을 것입니다.",
                "이제 일을 안 해도 되겠는걸요. 야호!"
            }, () => GoToCredits());
        }

        IEnumerator Ed5()
        {
            NarrationUI.Instance?.ShowNarrationSequence(new[]
            {
                "손자는 모든 것을 되돌렸습니다.\n할아버지가 파괴한 것들을.",
                "하지만 할아버지는 이미 죽었습니다.\n복수할 상대도, 용서할 상대도 없습니다."
            });

            yield return new WaitForSeconds(8f);

            yield return ShowDialogueSequence(new[]
            {
                ("손자", "이 책은… 거짓말이야."),
                ("손자", "거짓된 역사를 물려줄 수는 없으니까.")
            });

            ChoiceUI.Instance?.ShowChoices(
                new[] { "책을 불태운다." },
                _ => StartCoroutine(PostBookBurn())
            );
        }

        IEnumerator PostBookBurn()
        {
            AudioManager.Instance?.PlaySFX("SFX_BookBurn");
            AudioManager.Instance?.StopBGM(2f);

            yield return new WaitForSeconds(5f);

            NarrationUI.Instance?.ShowNarrationSequence(new[]
            {
                "손자는 사막인권운동가가 되어 떠납니다.",
                "할아버지가 회피한 것을 손자가 직시합니다.",
                "아무도 벌받지 않은 세상에서,\n손자만이 속죄하러 떠납니다.",
                "이것이 The SSand의 이야기입니다."
            });

            yield return new WaitForSeconds(8f);

            EndingManager.Instance?.TriggerBookBurn();
        }

        IEnumerator NGNormalEnding()
        {
            NarrationUI.Instance?.ShowNarrationSequence(new[]
            {
                "다 되지 않았습니다.",
                "씨앗을 전부 되돌리지 못했어요.",
                "아직 끝나지 않았습니다."
            }, () => SceneTransitionManager.Instance?.LoadScene("SCN_TitleMenu"));
            yield break;
        }

        IEnumerator ShowDialogueSequence((string speaker, string text)[] lines)
        {
            foreach (var line in lines)
            {
                NarrationUI.Instance?.ShowNarrationImmediate($"<b>{line.speaker}:</b> {line.text}");
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                yield return null;
            }
        }

        void GoToCredits()
        {
            StartCoroutine(CreditsRoutine());
        }

        IEnumerator CreditsRoutine()
        {
            yield return new WaitForSeconds(2f);
            SceneTransitionManager.Instance?.LoadScene("SCN_TitleMenu");
        }
    }
}
