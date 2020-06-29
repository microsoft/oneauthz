1. open solution Exterprise Security Demo solution.

2. Build Solution

3. Right click ITAuthorizeDemoExpensePortal project and Publish
to local folder Default folder will be ./bin/Release/PublishOutput.

4. Open dockerfile under ITAuthorizeDemoPet check if the copy from path is same as your published folder path.
EXAMPLE: COPY ./bin/Release/PublishOutput .

5. Start your docker Console.

6. CD to the same level folder as "dockerfile".

7. Run "docker build -t expensedemo ." (Don't forget the '.' in the end of command. That means where to find dockerfile) 

8. Run docker images expensedemo //Check image is created.

9. docker login "Your ACR url youracr.azurecr.io" -u "Your ACR Account" -p "Your ACR access Key" // login to Azure container Registory
E.X. docker login chaotestcontainerentry.azurecr.io -u chaoTestContainerEntry -p xxxxxxxxxx

10. docker tag expensedemo youracr.azurecr.io/yourImageName:tag
E.X. docker tag expenseportal chaotestcontainerEntry.azurecr.io/testdemo:1.0

11. docker push youracr.azurecr.io/yourImageName:tag
E.X. docker push chaotestcontainerEntry.azurecr.io/testdemo:1.0
