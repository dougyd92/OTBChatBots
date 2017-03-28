# Alliant Tech Support Bot

This is an automated technical support chatbot for [REAL Software Systems](http://www.realsoftwaresystems.com/)' Alliant platform. The bot was created using the Microsoft Bot Framework and runs as an Azure Bot Service. The chatbot provides a user-friendly, conversational interface as opposed to a simple search box. The user is able to discuss and describe their problem as if they were speaking with a human customer service representative. The backing SQL database holds all the known issues and troubleshooting steps. The bot uses semantic search to find the correct document to present to the user.

The bulk of the logic can be found in messages/EchoDialog.csx; the rest of the files are mostly part of the template project for the Azure Bot Service.

Created by Douglas de Jesus as part of REAL's Out of the Box Day 2016.
