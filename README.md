# [WIP] JJFramework

## 이것이 무엇인가요?

Unity에서 자주 사용했던, 유용했던 코드를 모아놓은 프레임워크입니다.

## 왜죠?

Unity로 개발하다보면 반복해서 쓰는 코드들이 종종 있었습니다. 매 프로젝트를 진행할 때마다 복사/붙여넣기를 하는 것이 효율적이지 못하다고 생각했습니다. 

> *자주 쓰는 코드라면 한 곳에 모아서 필요할 때 꺼내쓰면 되지 않을까?*

라는 생각해서 출발했습니다.

## 어떻게 하죠?

Unity 프로젝트의 Assets 폴더 안에 적절한 곳에

`git submodule add https://github.com/onsemy/JJFramework.git JJFramework`

를 실행하거나, 다운로드 받아서 넣어주세요!

## 환경 설정

- Unity 2017.2.3f1 이상 (.NET 4.6)

### 의존성 패키지

- [`UniRx`](https://github.com/neuecc/UniRx): Unity 프로젝트 내에 넣어주세요!
- [`JsonDotNet`](https://www.parentelement.com/assets/json_net_unity): Unity 프로젝트 내에 넣어주세요!

## 주요 기능

### Runtime

#### Attribute

- `ComponentPath`

#### Extension

- `UnityEngine.Component` 확장
- `UnityEngine.GameObject` 확장
- `UnityEngine.MonoBehaviour` 확장
- `string` 확장
- `List<T>` 확장

#### UI

- `BaseUI` 및 `UIManager`

#### Resource

- Resource 관리
  - Internal/External

#### Util

- `PlayerPrefsType`
- `TableToUIText`
- `WaitFor` 시리즈

### Editor

#### DB

