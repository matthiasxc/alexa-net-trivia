# Alexa Trivia Skill in .NET 
This is a C# port of the [Alexa Space Geek sample](https://github.com/alexa/skill-sample-nodejs-trivia) 

This is the code refereed to in my 3 part Alexa Skills tutorial series 

1. [AWS Setup for Alexa Skills](http://matthiasshapiro.com/2017/02/10/tutorial-alexa-skills-in-c-setup/)
2. [Writing an Alexa Skill in C#](http://matthiasshapiro.com/2017/02/10/tutorial-alexa-skills-in-c-the-code/) - a detailed overview of this project
3. [Deploying and Testing an Alexa Skill](http://matthiasshapiro.com/2017/02/10/tutorial-alexa-skills-in-c-setup/Deploying)

This skill utilizes the [Alexa Skills SDK for .NET](https://github.com/timheuer/alexa-skills-dotnet).

# Key Components #
There are two key parts to this project (and to any Alexa Skill running of Amazon's Lambda service).

The first is the code itself, which is in [Function.cs](https://github.com/matthiasxc/alexa-net-trivia/blob/master/SpaceGeek/Function.cs).

The second part are the Utterances and Intent Schema, which are both found in the [SkillAssets folder](https://github.com/matthiasxc/alexa-net-trivia/tree/master/SkillAssets). These are not compiled into the skill but are used when declaring and deploying the application through [Amazon's Alexa Skill portal.](https://developer.amazon.com/edw/home.html#/)
