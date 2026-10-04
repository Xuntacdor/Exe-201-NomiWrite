typedef Json = Map<String, dynamic>;

double? asDouble(dynamic value) =>
    value is num ? value.toDouble() : double.tryParse('$value');
int asInt(dynamic value, [int fallback = 0]) =>
    value is num ? value.toInt() : int.tryParse('$value') ?? fallback;
String asText(dynamic value, [String fallback = '']) =>
    value == null ? fallback : '$value';
List<Json> asJsonList(dynamic value) => value is List
    ? value.whereType<Map>().map((e) => Map<String, dynamic>.from(e)).toList()
    : [];

class AuthSession {
  const AuthSession({
    required this.accessToken,
    required this.refreshToken,
    required this.expiresAt,
    required this.userId,
    required this.email,
    required this.fullName,
    required this.role,
  });
  final String accessToken;
  final String refreshToken;
  final String expiresAt;
  final String userId;
  final String email;
  final String fullName;
  final String role;

  factory AuthSession.fromJson(Json json) => AuthSession(
    accessToken: asText(json['accessToken']),
    refreshToken: asText(json['refreshToken']),
    expiresAt: asText(json['expiresAt']),
    userId: asText(json['userId']),
    email: asText(json['email']),
    fullName: asText(json['fullName']),
    role: asText(json['role']),
  );
  Json toJson() => {
    'accessToken': accessToken,
    'refreshToken': refreshToken,
    'expiresAt': expiresAt,
    'userId': userId,
    'email': email,
    'fullName': fullName,
    'role': role,
  };
}

class WritingType {
  const WritingType({
    required this.id,
    required this.name,
    required this.category,
    required this.description,
  });
  final String id, name, category, description;
  factory WritingType.fromJson(Json j) => WritingType(
    id: asText(j['id']),
    name: asText(j['name'] ?? j['label']),
    category: asText(j['category'] ?? j['badge']),
    description: asText(j['description']),
  );
}

class WritingPrompt {
  const WritingPrompt({
    required this.id,
    required this.typeId,
    required this.typeName,
    required this.title,
    required this.instructions,
    required this.difficulty,
    this.imageUrl,
    this.minWords,
    this.maxWords,
  });
  final String id, typeId, typeName, title, instructions, difficulty;
  final String? imageUrl;
  final int? minWords, maxWords;
  factory WritingPrompt.fromJson(Json j) => WritingPrompt(
    id: asText(j['id']),
    typeId: asText(j['writingTypeId']),
    typeName: asText(j['writingTypeName'] ?? j['writingType']),
    title: asText(j['title'] ?? j['topic']),
    instructions: asText(j['instructions'] ?? j['prompt']),
    difficulty: asText(j['difficulty'], 'Intermediate'),
    imageUrl: j['imageUrl'] as String?,
    minWords: j['minWords'] == null ? null : asInt(j['minWords']),
    maxWords: j['maxWords'] == null ? null : asInt(j['maxWords']),
  );
}

class Submission {
  const Submission({
    required this.id,
    required this.promptId,
    required this.title,
    required this.content,
    required this.wordCount,
    required this.status,
    this.submittedAt,
    this.deadlineAt,
    this.overallScore,
  });
  final String id, promptId, title, content, status;
  final int wordCount;
  final String? submittedAt, deadlineAt;
  final double? overallScore;
  factory Submission.fromJson(Json j) => Submission(
    id: asText(j['id']),
    promptId: asText(j['writingPromptId']),
    title: asText(j['promptTitle'] ?? j['topic']),
    content: asText(j['content']),
    wordCount: asInt(j['wordCount']),
    status: asText(j['status']),
    submittedAt: j['submittedAt'] as String?,
    deadlineAt: j['deadlineAt'] as String?,
    overallScore: asDouble(j['overallScore']),
  );
}

class VocabularyItem {
  const VocabularyItem({
    required this.id,
    required this.original,
    required this.suggested,
    required this.example,
    required this.topic,
    required this.mastered,
  });
  final String id, original, suggested, example, topic;
  final bool mastered;
  factory VocabularyItem.fromJson(Json j) => VocabularyItem(
    id: asText(j['id']),
    original: asText(j['originalWord']),
    suggested: asText(j['suggestedWord']),
    example: asText(j['exampleSentence']),
    topic: asText(j['topic']),
    mastered: j['isMastered'] == true,
  );
}

class QuizSummary {
  const QuizSummary({
    required this.id,
    required this.category,
    required this.questionCount,
    this.latestScore,
    this.latestTotal,
  });
  final String id, category;
  final int questionCount;
  final double? latestScore;
  final int? latestTotal;
  factory QuizSummary.fromJson(Json j) => QuizSummary(
    id: asText(j['id']),
    category: asText(j['category'], 'Practice'),
    questionCount: asInt(j['questionCount']),
    latestScore: asDouble(j['latestScore']),
    latestTotal: j['latestTotalQuestions'] == null
        ? null
        : asInt(j['latestTotalQuestions']),
  );
}
