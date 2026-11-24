/*
 * 1.마우스를 클릭하면 Cat 플레이어가 점프하는 기능 구현
 *   1) Rigidbody2D 컴포넌트를 이용한 물리적 움직임 처리
 *   2) 마우스 왼쪽 버튼 클릭 감지
 *   3) AddForce() 메서드를 이용한 플레이어 점프
 *   4) 프레임레이트를 60FPS로 설정
 * 
 * 2. 플레이어를 오른쪽으로 움직이는 기능 구현
 *   ① AddForce() 메서드로 오른쪽 방향으로 힘을 가함
 *   ② 힘을 계속 가하면 플레이어의 이동 속도가 증가
 *   ③ linearVelocityX로 플레이어의 X축 이동 속도 확인
 *   ④ 최고 속도보다 느린 경우에만 오른쪽으로 힘을 가함
 *   
 * 3. 플립북 방식의 걷기 애니메이션
 *   ① 여러 장의 스프라이트 이미지를 배열에 저장
 *   ② Time.deltaTime으로 경과 시간 계산
 *   ③ 0.1초마다 스프라이트 이미지 변경
 *   ④ 배열 인덱스를 변경하여 애니메이션 반복
 *   
 * 4. 게임 씬에서 클리어 씬으로 전환하기
 *   ① 깃발(Flag) 오브젝트에 Collider2D 컴포넌트 추가
 *   ② Collider2D 컴포넌트의 Is Trigger 속성 활성화
 *   ③ OnTriggerEnter2D() 메서드로 플레이어와 깃발의 충돌 감지
 *   ④ CompareTag() 메서드로 충돌한 오브젝트가 깃발인지 확인
 *   ⑤ Debug.Log() 메서드로 "골!!" 메시지 출력
 *   ⑥ UnityEngine.SceneManagement 네임스페이스 추가
 *   ⑦ SceneManager.LoadScene() 메서드로 클리어 씬 전환
 *   ⑧ Build Profiles의 Scene List에 클리어 씬 등록
 */

using UnityEngine;

//****** 게임 씬에서 클리어 씬으로 전환하기
/*
 * [ SceneManagement 네임스페이스 ]
 * UnityEngine.SceneManagement
 *   - Unity에서 게임 씬(Scene)을 관리하기 위한 네임스페이스
 *   - 씬 불러오기, 씬 전환 등의 기능 제공
 * SceneManager
 *   - 게임 씬을 관리하는 클래스
 * SceneManager.LoadScene()
 *   - 지정한 이름 또는 인덱스의 씬을 불러오는 메서드
 *   - 게임 씬에서 클리어 씬으로 전환할 때 사용
 */
using UnityEngine.SceneManagement;  // LoadScene() 메서드를 사용하기 위해 필요한 네임스페이스

public class PlayerController : MonoBehaviour
{
    // 1. 멤버 변수 선언
    // Cat 오브젝트에 부착된 Rigidbody2D 컴포넌트를 저장할 변수
    // Rigidbody2D: 2D 물리 엔진에서 중력, 힘, 속도 등을 처리
    Rigidbody2D rigid2DCat = null;


    float playerJumpForce = 600.0f;

    // 플레이어를 오른쪽으로 이동시키기 위한 힘의 크기
    float playerWalkForce = 30.0f;

    // 플레이어의 x축 방향 최고 이동 속도
    float playerMaxWalkSpeed = 2.0f;

    //****** 6.6.2 플립북 방식의 애니메이션
    /*
     * [ Sprite 배열 ]
     * Sprite : 2D 게임에서 사용하는 이미지
     * walkSprites[]
     *   - 걷기 애니메이션에 사용할 스프라이트 배열
     *   - public으로 선언하여 Inspector에서 설정 가능
     *   - 배열에 등록된 이미지를 순서대로 재생
     */
    public Sprite[] walkSprites;

    /*
     * [ 애니메이션 경과 시간 ]
     * animationTime
     *   - 애니메이션 이미지 변경을 위한 시간 누적 변수
     *   - Time.deltaTime을 이용하여 경과 시간 계산
     */
    float animationTime = 0.0f;

    /*
     * [ 애니메이션 배열 인덱스 ]
     * animationIndex
     *   - 현재 표시할 스프라이트 배열의 인덱스
     *   - 배열 인덱스는 0부터 시작
     */
    int animationIndex = 0;

    /*
     * [ SpriteRenderer 컴포넌트 ]
     * SpriteRenderer
     *   - 2D 스프라이트 이미지를 화면에 출력
     *   - sprite 속성을 변경하여 이미지 교체
     */
    SpriteRenderer catSpriteRenderer = null;

    //****** 6.6.4 점프 중인 모습 추가            p283
    /*
     * [ 점프 애니메이션 스프라이트 ]
     * jumpSprites
     *   - Cat 플레이어가 점프 중일 때 표시할 이미지 저장
     *   - public으로 선언하여 Inspector 창에서 이미지 설정 가능
     *   - 플레이어가 상승하거나 하강할 때 점프 이미지 출력
     * ※ 걷기 애니메이션은 여러 장의 이미지를 사용하므로 Sprite[] 배열 사용
     * ※ 점프 이미지는 한 장만 사용하므로 Sprite 자료형 사용
     */
    public Sprite jumpSorites = null;

    // 게임 시작 시 한 번 실행
    void Start()
    {
        Application.targetFrameRate = 60;

        /*
         * [ GetComponent() 메서드]
         * 특정 게임 오브젝트에 부착된 컴포넌트를 가져오는 메서드
         * GetComponent<Rigidbody2D>()
         *   - 현재 Cat 오브젝트의 Rigidbody2D 컴포넌트를 검색
         *   - 검색한 컴포넌트를 rigid2DCat 변수에 저장
         * Rigidbody2D 컴포넌트를 이용하면
         *   - 중력 작용
         *   - 힘을 이용한 이동
         *   - 물리적 충돌 처리 등을 구현할 수 있음
         */
        rigid2DCat = GetComponent<Rigidbody2D>();

        //****** 6.6.2 플립북 방식의 애니메이션
        /*
         * [ SpriteRenderer 컴포넌트 가져오기 ]
         * GetComponent<SpriteRenderer>()
         *   - Cat 오브젝트의 SpriteRenderer 컴포넌트 검색
         *   - catSpriteRenderer 변수에 저장
         * SpriteRenderer.sprite
         *   - 현재 화면에 출력할 스프라이트 이미지
         */
        catSpriteRenderer = GetComponent<SpriteRenderer>();

    }

    // Update(): 매 프레임마다 실행
    void Update()
    {
        // 왼쪽마우스 버튼을 누르는 순간을 감지하는 메서드
        if (Input.GetMouseButtonDown(0))
        {
            /*
             *  [ AddForce() 메서드 ]
             *  Rigidbody2D 컴포넌트에 힘을 가하는 메서드
             *  사용 형식
             *    Rigidbody2D.AddForce(방향 * 힘의 크기);
             *  transform.up
             *    - 현재 오브젝트의 로컬 Y축 위쪽 방향
             *  playerJumpForce
             *    - 플레이어에 가할 힘의 크기(600.0f)
             *  transform.up * playerJumpForce
             *    - 위쪽 방향으로 600의 힘 벡터 생성
             *  AddForce()
             *    - 생성된 힘을 Rigidbody2D에 적용
             *    - 기본 ForceMode2D.Force 방식 사용
             *    - 플레이어가 위쪽으로 움직이도록 처리
             */
            rigid2DCat.AddForce(transform.up * playerJumpForce);
        }

        // 3-2. 플레이어를 오른쪽으로 움직이는 기능
        /*
         * [플레이어의 오른쪽 이동 알고리즘]
         * ① 플레이어의 현재 X축 이동 속도를 확인
         *    rigid2DCat.linearVelocityX
         *      - Rigidbody2D의 X축 방향 이동 속도
         * ② 최고 이동 속도와 비교
         *    playerMaxWalkSpeed = 2.0f
         * ③ 현재 속도가 최고 속도보다 느리면
         *    AddForce() 메서드로 오른쪽 방향의 힘을 가함
         * ④ 현재 속도가 최고 속도 이상이면
         *    추가적인 힘을 가하지 않음
         * ※ 힘을 가하지 않는다고 속도가 즉시 0이 되는 것은 아님
         */

        // 현재 X축 속도가 최고 이동 속도보다 느린 경우
        if (rigid2DCat.linearVelocityX < playerMaxWalkSpeed)
        {
            /*
             * [ AddForce() 메서드 : 오른쪽 이동 ]
             * transform.right : 플레이어의 오른쪽 방향 벡터
             * playerWalkForce : 오른쪽으로 이동할 때 가할 힘의 크기(30.0f)
             * transform.right * playerWalkForce
             *   - 오른쪽 방향으로 힘 벡터 생성
             * AddForce()
             *   - Rigidbody2D에 오른쪽 방향의 힘을 가함
             *   - 플레이어가 오른쪽으로 가속되도록 처리
             */
            rigid2DCat.AddForce(transform.right * playerWalkForce);
        }

        ////****** 6.6.2 플립북 방식의 애니메이션
        //// 3-3. 플립북 방식의 걷기 애니메이션
        ///*
        // * [ Time.deltaTime ]
        // * Time.deltaTime
        // *   - 이전 프레임부터 현재 프레임까지 경과한 시간
        // *   - 단위 : 초(second)
        // * animationTime += Time.deltaTime;
        // *   - 매 프레임 경과 시간을 누적
        // * 예)
        // *   60FPS 환경에서 한 프레임은 약 0.0167초
        // *   약 6프레임이 지나면 0.1초가 됨
        // */
        ////animationTime += Time.deltaTime;

        ///*
        // * [ 애니메이션 이미지 변경 조건 ]
        // * animationTime > 0.1f
        // *   - 누적 시간이 0.1초를 초과하면 실행
        // *   - 약 0.1초마다 스프라이트 이미지 변경
        // */
        //if (animationTime > 0.1f)
        //{
        //    // 애니메이션 시간 초기화
        //    animationTime = 0.0f;

        //    /*
        //     * [ 스프라이트 이미지 변경 ]
        //     * catSpriteRenderer.sprite
        //     *   - 현재 화면에 출력할 스프라이트 이미지
        //     * walkSprites[animationIndex]
        //     *   - 배열에서 해당 인덱스의 이미지 가져오기
        //     * 배열에 등록된 스프라이트가 있을 때만 실행
        //     */
        //    if (walkSprites != null && walkSprites.Length > 0)
        //    {
        //        catSpriteRenderer.sprite = walkSprites[animationIndex];

        //        /*
        //         * [ 배열 인덱스 변경 ]
        //         * animationIndex
        //         *   - 다음에 표시할 이미지의 인덱스
        //         * (animationIndex + 1) % walkSprites.Length
        //         *   - 인덱스를 1씩 증가
        //         *   - 배열의 마지막 이미지 다음에는 0으로 이동
        //         *   - 이미지 개수와 관계없이 반복 가능
        //         * 예) 이미지 2장
        //         *   0 → 1 → 0 → 1 ...
        //         * 예) 이미지 3장
        //         *   0 → 1 → 2 → 0 ...
        //         */
        //        animationIndex = (animationIndex + 1) % walkSprites.Length;
        //    }

        /*
        * [ 점프 상태에 따른 스프라이트 이미지 변경 ]
        * rigid2DCat.linearVelocityY
        *   - 플레이어의 Y축 방향 이동 속도
        *   - 양수 : 위쪽으로 이동(상승)
        *   - 음수 : 아래쪽으로 이동(하강)
        *   - 0에 가까움 : 수직 이동이 거의 없는 상태
        * Mathf.Abs()
        *   - 숫자의 절댓값을 반환하는 메서드
        * 0.01f
        *   - 미세한 속도 오차를 허용하기 위한 기준값
        */
        if (Mathf.Abs(rigid2DCat.linearVelocityY) > 0.01f)
        {
            // 점프 중인 스프라이트 이미지 출력
            catSpriteRenderer.sprite = jumpSorites;
        }
        else
        {
            //****** 6.6.2 플립북 방식의 애니메이션
            // 3-3. 플립북 방식의 걷기 애니메이션
            /*
             * [ Time.deltaTime ]
             * Time.deltaTime
             *   - 이전 프레임부터 현재 프레임까지 경과한 시간
             *   - 단위 : 초(second)
             * animationTime += Time.deltaTime;
             *   - 매 프레임 경과 시간을 누적
             * 예)
             *   60FPS 환경에서 한 프레임은 약 0.0167초
             *   약 6프레임이 지나면 0.1초가 됨
             */
            animationTime += Time.deltaTime;

            /*
             * [ 애니메이션 이미지 변경 조건 ]
             * animationTime > 0.1f
             *   - 누적 시간이 0.1초를 초과하면 실행
             *   - 약 0.1초마다 스프라이트 이미지 변경
             */
            if (animationTime > 0.1f)
            {
                // 애니메이션 시간 초기화
                animationTime = 0.0f;

                /*
                 * [ 스프라이트 이미지 변경 ]
                 * catSpriteRenderer.sprite
                 *   - 현재 화면에 출력할 스프라이트 이미지
                 * walkSprites[animationIndex]
                 *   - 배열에서 해당 인덱스의 이미지 가져오기
                 * 배열에 등록된 스프라이트가 있을 때만 실행
                 */
                if (walkSprites != null && walkSprites.Length > 0)
                {
                    catSpriteRenderer.sprite = walkSprites[animationIndex];

                    /*
                     * [ 배열 인덱스 변경 ]
                     * animationIndex
                     *   - 다음에 표시할 이미지의 인덱스
                     * (animationIndex + 1) % walkSprites.Length
                     *   - 인덱스를 1씩 증가
                     *   - 배열의 마지막 이미지 다음에는 0으로 이동
                     *   - 이미지 개수와 관계없이 반복 가능
                     * 예) 이미지 2장
                     *   0 → 1 → 0 → 1 ...
                     * 예) 이미지 3장
                     *   0 → 1 → 2 → 0 ...
                     */
                    animationIndex = (animationIndex + 1) % walkSprites.Length;
                }


            }
        }

        
    }
    /*
     * [ OnTriggerEnter2D() 메서드 ]
     *
     * 기능
     *   - 플레이어가 다른 2D Collider의 Trigger 영역에 진입했을 때 자동으로 호출되는 이벤트 메서드
     *   - 플레이어가 깃발에 닿았는지 감지
     *
     * OnTriggerEnter2D(Collider2D collision)
     *   - OnTriggerEnter2D : Trigger 충돌 이벤트 메서드
     *   - Collider2D : 충돌한 상대 오브젝트의 Collider2D 자료형
     *   - collision : 충돌한 상대 오브젝트의 Collider2D 정보
     *
     * Debug.Log()
     *   - Unity Console 창에 메시지를 출력하는 메서드
     *
     * 게임 동작
     *   ① 플레이어가 오른쪽으로 이동
     *   ② 플레이어가 깃발의 Trigger 영역에 진입
     *   ③ OnTriggerEnter2D() 메서드 자동 호출
     *   ④ Console 창에 "골!!" 메시지 출력
     */
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어가 Trigger 영역에 진입하면 메시지 출력
        Debug.Log("골!!");

        // 현재 게임 씬을 클리어 씬으로 전환
        SceneManager.LoadScene("ClearScene");
    }
}
